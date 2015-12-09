importScripts('/bower_installer/lamejs/lamejs.min.js');

// Worker runs in an isolated environment which has no proper <window>, it has <self> instead.

// polyfill for IE
(<any>self).Math.log10 = (<any>self).Math.log10 || function (x) {
    return Math.log(x) / Math.LN10;
};

declare function postMessage(message: any, ports?: any): void; // This is  DedicatedWorkerGlobalScope.postMessage()

interface ILamejsMp3Encoder {
    encodeBuffer: (left: Int16Array, right?: Int16Array) => Int8Array;
    flush: () => Int8Array;
}

interface ILamejs {
    Mp3Encoder: new (channels: number, samplerate: number, kbps: number) => ILamejsMp3Encoder;
}

// Declared in lame.js
declare var lamejs: {
    new (): ILamejs;
}

module app {

    export interface IEncoderConfig {
        command: string;
        channels: number;
        originalSampleRate: number; // before resampling
        sampleRate: number; // resampled
        bitRate: number;
    }

    var lamejsInstance: ILamejs = lamejsInstance || new lamejs();
    var config: IEncoderConfig;
    var encoder: Encoder;
    var resampler: Resampler;

    //(<any>self).my_onmessage = function (ev: MessageEvent) {
    self.onmessage = function (ev: MessageEvent) {
        //console.log(!!ev.data);
        if (ev.data.byteLength) {
            if (encoder && resampler) {
                var original: Float32Array = ev.data;
                var resampled = resampler.resample(original);
                encoder.addSamples(resampled);
            }
        }
        else {
            switch (ev.data.command) {
                case 'ping':
                    postMessage(null); // The main script is waiting for a response to make sure the worker is ready.
                    break;
                case 'init':
                    config = ev.data;
                    encoder = new Encoder(lamejsInstance, config);
                    resampler = new Resampler(config);
                    break;
                case 'finish':
                    if (encoder) {
                        encoder.finish();
                        encoder = null;
                    }
                    break;
            }
        }
    };

    class Encoder {

        private encoder: ILamejsMp3Encoder;
        private sampleChunks: Int16Array[] = [];
        private sampleChunk: Int16Array;
        private sampleChunkIndex: number = 0;
        private sampleBufferLength = 1152; //can be anything but make it a multiple of 576 to make encoder's life easier
        private sampleBuffer: Int16Array = new Int16Array(this.sampleBufferLength);
        private sampleBufferIndex: number = 0;

        constructor(lamejsInstance: ILamejs, config: IEncoderConfig) {
            this.encoder = new lamejsInstance.Mp3Encoder(config.channels, config.sampleRate, config.bitRate);
        }

        // Flash and getUserMedia send PCM32 as native JS numbers representing floats in range -1.0...1.0 .
        // Mp3Encoder expects PCM16, integers in range -32768...32767 .
        addSamples = (floatArray: Float32Array) => {
            //console.log(floatArray.byteLength);
            var intArray = new Int16Array(floatArray.length);
            for (var i = 0; i < floatArray.length; i++) {
                var float = floatArray[i];
                var word = float < 0 ? float * 32768 : float * 32767;
                intArray[i] = Math.max(-32768, Math.min(32768, Math.round(word)));
            }
            this.sampleChunks.push(intArray);
            this.processSamples();
        };

        finish = () => {
            this.processSamples(true);
        };

        private processSamples = (finish?: boolean) => {
            while (this.sampleChunk || (this.sampleChunks.length != 0) || finish) {
                if (!this.sampleChunk) {
                    this.sampleChunk = this.sampleChunks.shift(); // returns undefined if length is 0;
                }
                if (this.sampleChunk) {
                    var lengthToCopy = Math.min(this.sampleBufferLength - this.sampleBufferIndex, this.sampleChunk.length - this.sampleChunkIndex);
                    var samplesToCopy = this.sampleChunk.subarray(this.sampleChunkIndex, this.sampleChunkIndex + lengthToCopy);
                    this.sampleBuffer.set(samplesToCopy, this.sampleBufferIndex);
                    this.sampleChunkIndex += lengthToCopy;
                    this.sampleBufferIndex += lengthToCopy;
                    if (this.sampleChunkIndex === this.sampleChunk.length) {
                        this.sampleChunk = null;
                        this.sampleChunkIndex = 0;
                    }
                }
                var isFinal = !this.sampleChunk && (this.sampleChunks.length === 0) && finish;
                if ((this.sampleBufferIndex === this.sampleBufferLength) || isFinal) {
                    this.sampleBufferIndex = 0;
                    var page = this.encoder.encodeBuffer(this.sampleBuffer);
                    // Typed arrays can be passed directly as transferrable objects without serializing/deserializing by using the special signature of postMessage()
                    postMessage(page, [page.buffer]);
                    if (isFinal) {
                        finish = false;
                        page = this.encoder.flush();
                        postMessage({ page: page, isFinal: isFinal }, [page.buffer]);
                    }
                }
            }
        };

    } // end of class Encoder

    // Copied from +https://github.com/chris-rudmin/Recorderjs/blob/master/resampler.js
    // There is another sophisticated resampler at +https://github.com/taisel/XAudioJS/blob/master/resampler.js
    class Resampler {
        originalRate: number;
        resampledRate: number;
        lastSampleCache: number[] = [0, 0];

        constructor(config: IEncoderConfig) {
            this.originalRate = config.originalSampleRate;
            this.resampledRate = config.sampleRate;
        };

        // From +http://johncostella.webs.com/magic/
        magicKernel(x) {
            if (x < -0.5) {
                return 0.5 * (x + 1.5) * (x + 1.5);
            }
            else if (x > 0.5) {
                return 0.5 * (x - 1.5) * (x - 1.5);
            }
            return 0.75 - (x * x);
        };

        resample = (inBuffer: Float32Array) => {
            if (this.resampledRate === this.originalRate) {
                return inBuffer;
            }

            var outBufferLength = Math.round(inBuffer.length * this.resampledRate / this.originalRate);
            var resampleRatio = inBuffer.length / outBufferLength;
            var outBuffer = new Float32Array(outBufferLength);

            for (var i = 0; i < outBufferLength - 1; i++) {
                var resampleValue = (resampleRatio - 1) + (i * resampleRatio);
                var nearestPoint = Math.round(resampleValue);

                for (var tap = -1; tap < 2; tap++) {
                    var sampleValue = inBuffer[nearestPoint + tap] || this.lastSampleCache[1 + tap] || inBuffer[nearestPoint];
                    outBuffer[i] += sampleValue * this.magicKernel(resampleValue - nearestPoint - tap);
                }
            }

            this.lastSampleCache[0] = inBuffer[inBuffer.length - 2];
            this.lastSampleCache[1] = outBuffer[outBufferLength - 1] = inBuffer[inBuffer.length - 1];

            return outBuffer;
        };

    } // end of class Resampler

} // end of module app