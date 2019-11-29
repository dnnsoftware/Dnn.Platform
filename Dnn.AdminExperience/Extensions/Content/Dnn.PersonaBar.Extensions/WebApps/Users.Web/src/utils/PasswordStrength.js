/* eslint-disable no-useless-escape */
export const PasswordStrengthType = {
    WEAK: "weak",
    FAIR: "fair",
    STRONG: "strong"
};

export const getPasswordStrength = (password, passwordStrengthOptions) => {
    if (passwordStrengthOptions === undefined || passwordStrengthOptions === null) {
        return null;
    }

    let pStrengthRating = passwordStrengthRating(password,passwordStrengthOptions);
   
    if (password.length <= 2 ) {
        return null;
    }
    if (pStrengthRating.rating < 3 ) {
        return PasswordStrengthType.WEAK;
    }
    if (pStrengthRating.rating < 5) {
        return PasswordStrengthType.FAIR;
    }
    if (pStrengthRating.rating >= 5) {
        return PasswordStrengthType.STRONG ;
    }
};

export const passwordStrengthRating = (password, options) => {
    let rating = 0;

    //this next property will be initialised with a server value
    let minLength = options.minLength;

    let hasOneUpperCaseChar = false;
    let hasOneLowerCaseChar = false;
    let hasMinNumberOfSpecialChars = false;
    let hasOneNumericChar = false;
    let hasLengthOfNChars = false;
    let matchValidationExpression = false;

    let minNumberOfSpecialChars = options.minNumberOfSpecialChars || 0;
    let validationExpression = options.validationExpression || "";

    if (password.length > 0) {

        if (password.match(/[a-z]/)) {
            rating++;
            hasOneLowerCaseChar = true;
        }
        if (password.match(/[A-Z]/)) {
            rating++;
            hasOneUpperCaseChar = true;
        }
        if (password.match(/[0-9]/g)) {
            rating++;
            hasOneNumericChar = true;
        }

        let matches = password.match(/[!,@,#,$,%,&,*,(,),\-,_,=,+,\',\",\\,|,\,,<,.,>,;,:,/,?,\[,{,\],}]/g);
        if (matches && matches.length >= minNumberOfSpecialChars) {
            rating++;
            hasMinNumberOfSpecialChars = true;
        }

        if (password.length >= minLength) {
            rating++;
            hasLengthOfNChars = true;
        }

        if (password.length >= minLength + 3) {
            rating++;
        }

        if (validationExpression) {
            matchValidationExpression = new RegExp(validationExpression, "g" ).test(password);
        }
    }
    return {
        rating: rating,
        maxRating: 5,
        hasOneUpperCaseChar: hasOneUpperCaseChar,
        hasOneLowerCaseChar: hasOneLowerCaseChar,
        hasMinNumberOfSpecialChars: hasMinNumberOfSpecialChars,
        hasOneNumericChar: hasOneNumericChar,
        hasLengthOfNChars: hasLengthOfNChars,
        matchValidationExpression: matchValidationExpression
    };
};