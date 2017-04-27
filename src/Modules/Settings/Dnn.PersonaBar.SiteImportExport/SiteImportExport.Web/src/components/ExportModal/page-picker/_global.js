export const global = {
    styles: {
        display: (type = "block") => {
            return {
                display: type
            };
        },

        listStyle: (type = "none") => {
            return {
                listStyle: type
            };
        },

        textAlign: (direction) => {
            return {
                textAlign: direction
            };
        },

        float: (direction = "left") => {
            return {
                float: direction
            };
        },

        clearFix: (direction) => {
            return {
                clear: direction
            };
        },

        padding: ({
            top = 0,
            left = 0,
            right = 0,
            bottom = 0,
            all = undefined,
            horizontal = undefined,
            vertical = undefined
        }) => {
            return {
                padding: ` ${all||vertical||top}px
                                ${all||horizontal||right}px
                                ${all||vertical||bottom}px
                                ${all||horizontal||left}px`
            };
        },

        margin: ({
            top = 0,
            left = 0,
            right = 0,
            bottom = 0,
            all = undefined,
            horizontal = undefined,
            vertical = undefined
        }) => {
            return {
                margin: `
                              ${all||vertical||top}px
                              ${all||horizontal||right}px
                              ${all||vertical||bottom}px
                              ${all||horizontal||left}px`
            };
        },

        width: (val = 100, unit = "%") => {
            return {
                width: `${val}${unit}`
            };
        },

        height: (val = 100, unit = "px") => {
            return {
                height: `${val}${unit}`
            };
        },

        backgroundColor: (string = "orange") => {
            return {
                backgroundColor: string
            };
        },

        merge: (...objs) => Object.assign({}, ...objs)
    }

};
