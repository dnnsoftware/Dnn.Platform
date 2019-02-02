//global compilation method
function compileTsSource(source, tsLib) {
    var compiler = new TsCompiler(tsLib);
    compiler.compile(source);
    return compiler.getResult();
}

//compiler class, accepts the TypeScript lib.d.ts string
function TsCompiler(tsLib) {
    
    //internal props
    this.output = "";
    this.libfile = tsLib;
    var self = this;

    //public class
    var obj = {
        compile: function (tsSource) {

            var outfile = {
                source: tsSource,
                Write: function (s) {
                    self.output += s;
                },
                WriteLine: function (s) {
                    self.output += s + '\n';
                },
                Close: function () {
                }
            };

            var compiler = new TypeScript.TypeScriptCompiler(outfile);
            var parseErrors = [];
            
            compiler.parser.errorRecovery = true;
            compiler.setErrorCallback(function (start, len, message, block) {
                parseErrors.push({
                    start: start,
                    len: len,
                    message: message,
                    block: block
                });
            });
            
            compiler.addUnit(self.libfile, 'lib.d.ts');
            compiler.addUnit(tsSource, '');

            compiler.typeCheck();
            
            compiler.emit(false, function () {
                return outfile;
            });
            
            //check for errors
            if (parseErrors.length > 0) {
                self.output = "TsCompiler - Compilation errors (" + parseErrors.length + "): \n\n";
                for (var e in parseErrors) {
                    self.output += parseErrors[e].message + '\nCode block: ' + parseErrors[e].block + '\nStart position: ' + parseErrors[e].start + '\nLength: ' + parseErrors[e].len + "\n\n";
                }
            }

        },
        getResult: function () {
            return self.output;
        }
    };

    return obj;
};