let instance = null;

export default class DomKey {

    constructor() {
        if (!instance) {
            instance = this;
            this.key = 0;
        }

        return instance;
    }

    static get(prefix = "") {
        const self = instance ? instance : new DomKey();
        return `${prefix}-${self.key++}`;
    }
}
