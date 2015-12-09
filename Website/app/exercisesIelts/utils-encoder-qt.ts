importScripts('/bower_installer/datastreamjs/DataStream.js');

declare function postMessage(message: any, ports?: any): void; // This is  DedicatedWorkerGlobalScope.postMessage()

module app {

    // Worker runs in an isolated environment which has no proper "window", it has "self" instead.
    self.onmessage = function (ev: MessageEvent) {
        if (ev.data.byteLength) {
            var encoder = new QTStripper();
            var original: ArrayBuffer = ev.data;
            var stripped = encoder.strip(original);
            postMessage(stripped, [stripped]);
        }
        else {
            switch (ev.data.command) {
                case 'ping':
                    postMessage(null); // The main script is waiting for a response to make sure the worker is ready.
                    break;
            }
        }
    };

    // The file extention is .MOV, the file format is QuickTime.

    interface IQTAtom {
        size: number;
        type: number; // Although it is usually represented by a mnemonic value, it may contain non-ASCII values.
        // NOT data?: any; // Otherwise it must be not optional. If any field is null, DataStream.js considers it a failure to read the structure.
    }

    interface IAtomLocation extends IQTAtom {
        offset: number; // from the beginning of the file.
    }

    interface IChunk {
        offset: number;
        newOffset: number;
        startSample: number;
        sampleCount: number;
        size: number; // bytes. The total byteLength of all samples in the chunk.
    }

    interface IParsedMetadata {
        atomLocations: IAtomLocation[],
        chunks: IChunk[],
    }

    class QTStructs {
        static AtomHeader = [
            'size', 'uint32',
            'type', 'uint32', // Although it is usually represented by a mnemonic value, it may contain non-ASCII values.
        ];
        // Sample-to-Chunk
        static stsc = [
            'skip', 'uint32',
            'count', 'uint32',
            'entries', ['[]',
                [
                    'chunk', 'uint32', // First chunk
                    'count', 'uint32', // Samples per chunk
                    'sdi', 'uint32' // Sample description ID
                ],
                'count']
        ];
        // Sample Size
        static stsz = [
            'skip', 'uint32',
            'sameSize', 'uint32',
            'count', 'uint32',
            'entries', ['[]', 'uint32', 'count']
        ];
        // Chunk Offset
        static stco = [
            'skip', 'uint32',
            'count', 'uint32',
            'entries', ['[]', 'uint32', 'count']
        ];
    }

    class QTTypes {
        static ftyp = 1718909296; // 'ftyp', 66 74 79 70 
        static wide = 2003395685; // 'wide', 77 69 64 65
        static mdat = 1835295092; // 'mdat', 6D 64 61 74 
        static moov = 1836019574; // 'moov', 6D 6F 6F 76
        static trak = 1953653099; // 'trak', 7472616b
        static mdia = 1835297121; // 'mdia', 6d646961
        static minf = 1835626086; // 'minf', 6d696e66
        static stbl = 1937007212; // 'stbl', 7374626c
        static stsd = 1937011556; // 'stsd', 73747364
        static stts = 1937011827; // 'stts', 73747473
        static stsc = 1937011555; // 'stsc', 73747363
        static stsz = 1937011578; // 'stsz', 7374737a
        static stco = 1937007471; // 'stco', 7374636f
        static free = 1718773093; // 'free', 66 72 65 65

        /*
        'tkhd' ‭746B6864 1953196132
        'edts' 65647473 ‭1701082227‬
        'mdhd' 6D646864 ‭1835296868‬
        'hdlr' 68646C72 ‭1751411826‬
        'vmhd' 766D6864 ‭1986881636‬
        'smhd' ‭736D6864‬ ‭1936549988‬
        'dinf' 64696E66 ‭1684631142‬
        'stss' 73747373 ‭1937011571‬
        
        */

        //static hex2a = function (value: number) {
        //    var hex = value.toString(16);
        //    var str = '';
        //    for (var i = 0; i < hex.length; i += 2)
        //        str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
        //    return str;
        //};
    };

    class QTConsts {
        static MajorBrand = 1903435808; // 'qt  ', 71 74 20 20 
        static DataFormat = 1836069985; // Audio 'mp4a' (Video is 1635148593 "avc1")
    };

    export class QTStripper {

        constructor(
        ) {
        } // end of ctor 

        /**
        * Copy a QuickTime file contents over to a new ArrayBuffer. Copy the audio only, strip the video.
        */
        strip = (buffer: ArrayBuffer) => {
            var srcMetadata = this.parse(buffer);
            return this.copy(buffer, srcMetadata);
        };

        private parse = (srcBuffer: ArrayBuffer) => {
            var ds = new DataStream(srcBuffer, 0, DataStream.BIG_ENDIAN);

            var isAudioTrack = false;
            var tables: any = {};
            var atomLocations: IAtomLocation[] = [];
            var chunks: IChunk[] = [];

            while (!ds.isEof()) {
                var position: number = (<any>ds).position;
                var atom = <IQTAtom>ds.readStruct(QTStructs.AtomHeader);
                //console.log(position, position.toString(16), atom, atom.size.toString(16), atom.type.toString(16), QTTypes.hex2a(atom.type));

                if (position === 0) {
                    var value = ds.readUint32();
                    if (atom.type != QTTypes.ftyp || value != QTConsts.MajorBrand) {
                        toastr.error('Unexpected file type.');
                        return;
                    }
                }

                // Remember the original locations of the most important atoms. We will relocate some of them.
                // There is another 'stco' for a Video track.
                if ((atom.type === QTTypes.ftyp)
                    || (atom.type === QTTypes.moov)
                    || (atom.type === QTTypes.trak)
                    || ((atom.type === QTTypes.stco) && isAudioTrack)
                ) {
                    atomLocations.push({
                        type: atom.type,
                        offset: position,
                        size: atom.size,
                    });
                }

                switch (atom.type) {
                    // These are parent/container atoms. We stay within them to iterate further through the child atoms.
                    case QTTypes.moov:
                    case QTTypes.trak:
                    case QTTypes.mdia:
                    case QTTypes.minf:
                    case QTTypes.stbl:
                        break;
                    case QTTypes.stsd:
                        ds.readUint32Array(3); // skip
                        // Distinguish between the audio Sample Table and the video one.
                        isAudioTrack = ds.readUint32() === QTConsts.DataFormat;
                        ds.seek(position + atom.size);
                        break;
                    case QTTypes.stsc:
                    case QTTypes.stsz:
                    case QTTypes.stco:
                        if (isAudioTrack) {
                            var structDef;
                            switch (atom.type) {
                                case QTTypes.stsc:
                                    structDef = QTStructs.stsc;
                                    break;
                                case QTTypes.stsz:
                                    structDef = QTStructs.stsz;
                                    break;
                                case QTTypes.stco:
                                    structDef = QTStructs.stco;
                            }
                            var st = ds.readStruct(structDef);
                            tables[atom.type] = st['entries'];
                        }
                    // No break; We seek to the next atom on the same level.
                    case QTTypes.ftyp:
                    default:
                        ds.seek(position + atom.size);
                        break;
                };
            }

            var chunkSizes: any[] = tables[QTTypes.stsc];
            var sampleSizes: Uint32Array = tables[QTTypes.stsz];
            var chunkOffsets: Uint32Array = tables[QTTypes.stco];

            // chunkSizes.length does not have to be equal to chunkOffsets.length though. See the explanation below.
            // Calculate sample range for every chunk.
            var sampleCount = chunkSizes.reduce((previous: number, item: any, index: number, array: any[]) => {
                // An entry in chunkSizes can correspond to a set of consecutive chunks. The table is sparse. We create a sequential 0-based array.
                // fromChunk including, toChunk excluding. According to the format, the chunk indexes in the chunkSizes table are 1-based. 
                var fromChunk = item.chunk;
                var toChunk = (index + 1 < array.length)
                    ? array[index + 1].chunk
                    : chunkOffsets.length + 1;
                for (var i = fromChunk; i < toChunk; i++) {
                    chunks.push(<IChunk>{
                        startSample: previous,
                        sampleCount: item.count,
                    });
                    previous += item.count;
                }
                return previous;
            }, 0);

            if (chunks.length !== chunkOffsets.length) {
                toastr.error('Chunk count is wrong.');
                return;
            }

            if (sampleCount !== sampleSizes.length) {
                toastr.error('Sample count is wrong.');
                return;
            }

            // Calculate the size of all samples in every chunk and copy offset from the original table.
            chunks.forEach((i, idx) => {
                // Uint32Array in Safari iOS has neither slice() nor reduce().
                var sizes = sampleSizes.subarray(i.startSample, i.startSample + i.sampleCount);
                var total = 0;
                for (var j = 0; j < sizes.length; j++) {
                    total += sizes[j];
                }
                i.size = total;

                i.offset = chunkOffsets[idx];
            });

            return <IParsedMetadata>{
                atomLocations: atomLocations,
                chunks: chunks,
            }
        };

        private copy = (srcBuffer: ArrayBuffer, metadata: IParsedMetadata) => {

            function arrFind<T>(arr: T[], test: (value: T, index: number, array: T[]) => boolean, ctx?: any) {
                var result: T = null;
                (arr || []).some(function (value, index) {
                    return test.call(ctx, value, index, arr) ? ((result = value), true) : false;
                });
                return result;
            }
            var getLocation = (type: number) => {
                return arrFind(metadata.atomLocations, (i) => { return i.type === type; });
            };

            var getLocation2 = (type: number) => {
                return arrFind(metadata.atomLocations, (i) => { return i.type === type; });
            };

            var ftyp = getLocation(QTTypes.ftyp);
            var moov = getLocation(QTTypes.moov);
            var stco = getLocation(QTTypes.stco);

            // The QuickTime format allows 'moov' to preceede 'mdat' (and thus 'wide'). Do not make assumptions regarding the original file layout.
            /* The new file layout:
            ftyp
            wide
            mdat
            moov (/trak/mdia/minf/stbl/stco)
            */
            var totalChunkSize = metadata.chunks.reduce((previous: number, item: IChunk) => {
                return previous + item.size;
            }, 0);
            var dstLength = ftyp.size + /* atom of type'wide' */ 8 + /* 'mdat' header */ 8 + totalChunkSize + moov.size;

            var dstBuffer = new ArrayBuffer(dstLength);
            var ds = new DataStream(dstBuffer, 0, DataStream.BIG_ENDIAN);

            var memCopy = (srcOffset: number, dstOffset: number, byteLength: number) => {
                var srcArr = new Uint8Array(srcBuffer, srcOffset, byteLength);
                var dstArr = new Uint8Array(dstBuffer, dstOffset, byteLength);
                dstArr.set(srcArr);
            };

            memCopy(ftyp.offset, 0, ftyp.size);
            ds.seek(ftyp.size);
            ds.writeStruct(QTStructs.AtomHeader, { size: 8, type: QTTypes.wide, });
            ds.writeStruct(QTStructs.AtomHeader, { size: 8 + totalChunkSize, type: QTTypes.mdat, });
            // Calculate new chunk offsets. Copy and relocate the chunks.
            // The chunk offsets are file offsets, not the offset into any atom within the file (for example, a 'mdat' atom).           
            // The chunks offsets in the test file do not respect memory alignment (there are values like 0x146D1, 0x1E447)
            var moovNewOffset = metadata.chunks.reduce((previous: number, item: IChunk) => {
                item.newOffset = previous;
                memCopy(item.offset, item.newOffset, item.size);
                return previous + item.size;
            }, (<any>ds).position);
            memCopy(moov.offset, moovNewOffset, moov.size);
            // Seek to the Chunk Offset Table
            ds.seek(moovNewOffset + (stco.offset - moov.offset) + 16);
            // Write the new chunk offsets.
            metadata.chunks.forEach((i) => {
                ds.writeUint32(i.newOffset);
            });

            // Make the the metadata for other tracks except the audio one unaccessable.
            metadata.atomLocations
                .filter((i, idx, arr) => {
                    // The Audio 'trak' has an immidiatelly following 'stco';                   
                    return (i.type === QTTypes.trak) && ((idx === arr.length - 1) || (arr[idx + 1].type !== QTTypes.stco))
                })
                .forEach((i) => {
                    // Make the atom name 'traK' to render it invalid.
                    ds.seek(moovNewOffset + (i.offset - moov.offset) + 7);
                    ds.writeUint8(75); // Dec 75 = Hex 4B = 'K'
                });

            return dstBuffer;
        };

    } // end of class QtStripper  

} // end of module app