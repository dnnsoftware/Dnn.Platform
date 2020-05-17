import {
    passwordStrengthRating,
    getPasswordStrength,
    PasswordStrengthType
} from "./PasswordStrength";

describe("Util PasswordStrength ", () => {
    const defaultOptions = {
        minLength: 7,
        minNumberOfSpecialChars: 1,
        validationExpression: ""
    };

    const strongPassword = "Asdf1234!";
    const fairPasswordRating4 = "Asdf1234";
    const fairPasswordRating3 = "asdf1234";
    const weakPassword = "asdf";

    describe("PasswordStrengthRating", () => {
        it("Should password Asdf1234! be rating 5", () => {
            const password = strongPassword;
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(5);
        });

        it("Should password Asdf1234 be rating 4", () => {
            const password = fairPasswordRating4;
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(4);
        });

        it("Should password asdf1234 be rating 3", () => {
            const password = fairPasswordRating3;
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(3);
        });

        it("Should password asdf1 to be rating 2", () => {
            const password = "asdf1";
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(2);
        });

        it("Should password asdf be rating 1", () => {
            const password = weakPassword;
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(1);
        });

        it("Should password length has minLength+3, increase the rating", () => {
            const password = "asdfasdffg";
            expect(
                passwordStrengthRating(password, defaultOptions).rating
            ).toBe(3);
        });

        it("Should validate regular expression when it is present on configuration", () => {
            const password = "Asd1234!asdf";
            const strongRegex = new RegExp(
                "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])(?=.{8,})"
            );
            const regExpOptions = {
                ...defaultOptions,
                validationExpression: strongRegex
            };
            expect(passwordStrengthRating(password, regExpOptions).rating).toBe(
                6
            );
        });

        it("Should calculate password strength if no minimal special char is defined in password strength options", () => {
            const defaultOptions = {
                ...defaultOptions,
                minNumberOfSpecialChars: 0
            };
            const password = strongPassword;
            expect(passwordStrengthRating(password, defaultOptions).rating).toBe(4);
        });
    });

    describe("Password Strength Type", () => {
        it("Should return null when no password defined", () => {
            const pStrength = getPasswordStrength("", defaultOptions);
            expect(pStrength).toBe(null);
        });

        it("Should return weak on weak password", () => {
            const pStrength = getPasswordStrength(weakPassword, defaultOptions);
            expect(pStrength).toBe(PasswordStrengthType.WEAK);
        });

        it("Should return fair on fair password with rating 3", () => {
            const pStrength = getPasswordStrength(
                fairPasswordRating3,
                defaultOptions
            );
            expect(pStrength).toBe(PasswordStrengthType.FAIR);
        });

        it("Should return fair on fair password with rating 4", () => {
            const pStrength = getPasswordStrength(
                fairPasswordRating4,
                defaultOptions
            );
            expect(pStrength).toBe(PasswordStrengthType.FAIR);
        });

        it("Should return strong on strong password with rating 5 or more", () => {
            const pStrength = getPasswordStrength(
                strongPassword,
                defaultOptions
            );
            expect(pStrength).toBe(PasswordStrengthType.STRONG);
        });

        it("Should aways return null if no password options is defined", () => {
            const pStrength = getPasswordStrength(strongPassword, undefined);
            expect(pStrength).toBe(null);
        });
    });
});
