/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
    grunt.initConfig({
        bower: {
            install: {
                options: {
                    targetDir: './lib',
                    layout: 'byComponent',
                    install: true,
                    cleanTargetDir: false,
                    cleanBowerDir: false,
                    verbose: true,
                    bowerOptions: {
                        forceLatest: true,    // Force latest version on conflict
                        production: true,     // Do not install project devDependencies
                    }
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-bower-task');
};