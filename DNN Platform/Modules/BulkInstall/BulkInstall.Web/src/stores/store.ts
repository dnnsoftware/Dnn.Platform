import { createStore } from "@stencil/store";
import { BulkInstallLocalization } from "../clients/localization-client";

const { state } = createStore<IBulkInstallState>({
    resx: undefined,
    moduleId: undefined,
});

interface IBulkInstallState {
    resx: BulkInstallLocalization;
    moduleId: number;
}

export default state;