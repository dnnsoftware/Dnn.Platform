const string = {
    format() {
        let format = arguments[0];
        let methodsArgs = arguments;
        return format.replace(/{(\d+)}/gi, function (value, index) {
            let argsIndex = parseInt(index) + 1;
            return methodsArgs[argsIndex];
        });
    }
};
export default string;