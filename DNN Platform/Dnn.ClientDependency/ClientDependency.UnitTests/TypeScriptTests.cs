using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.TypeScript;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class TypeScriptTests
    {
        [Test]
        public void Can_Create_Engine()
        {
            var compiler = new TypeScriptWriter();
            var engine = compiler.GetEngine();
            Assert.NotNull(engine);
        }

        [Test]
        public void Can_Get_Successfull_Result()
        {
            var compiler = new TypeScriptWriter();
            var engine = compiler.GetEngine();

            var test = @"function greeter(person: string) {
                            return ""Hello, "" + person;
                        }
                        var user = ""Jane User"";
                        document.body.innerHTML = greeter(user);";

            var result = compiler.CompileTypeScript(engine, test);
            Assert.IsFalse(TypeScriptCompilationErrorParser.HasErrors(result));            
        }

        [Test]
        public void Can_Parse_Exception()
        {
            var exception = @"TsCompiler - Compilation errors (14): 

Expected ';'
Code block: 1
Start position: 8
Length: 7

Expected ')'
Code block: 1
Start position: 22
Length: 1

Expected ';'
Code block: 1
Start position: 22
Length: 1

Check format of expression term
Code block: 1
Start position: 22
Length: 1

Expected ';'
Code block: 1
Start position: 31
Length: 1

Check format of expression term
Code block: 1
Start position: 31
Length: 1

Expected ';'
Code block: 1
Start position: 33
Length: 1

return statement outside of function body
Code block: 1
Start position: 64
Length: 6

Expected '}'
Code block: 1
Start position: 234
Length: 1

The name 'functio' does not exist in the current scope
Code block: 1
Start position: 0
Length: 7

The name 'greeter' does not exist in the current scope
Code block: 1
Start position: 8
Length: 7

The name 'person' does not exist in the current scope
Code block: 1
Start position: 16
Length: 6

The name 'stringz' does not exist in the current scope
Code block: 1
Start position: 24
Length: 7

The name 'greeter' does not exist in the current scope
Code block: 1
Start position: 220
Length: 7
";
            Assert.IsTrue(TypeScriptCompilationErrorParser.HasErrors(exception));
            var errors = TypeScriptCompilationErrorParser.Parse(exception);
            Assert.AreEqual(14, errors.Count());
        }

        [Test]
        public void Can_Get_Exception_Result()
        {
            var compiler = new TypeScriptWriter();
            var engine = compiler.GetEngine();

            var test = @"functio greeter(person: stringz) {
                                        return ""Hello, "" +=-+ person;
                                    
                                    var user = ""Jane User"";
                                    document.body.innerHTML = greeter(user);";


            Assert.Throws<TypeScriptCompilationException>(() => compiler.CompileTypeScript(engine, test));            
        }

    }
}
