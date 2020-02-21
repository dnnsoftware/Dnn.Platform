import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import PersonaBarPage from "./index";
import PersonaBarPageHeader from "../PersonaBarPageHeader";
import PersonaBarPageBody from "../PersonaBarPageBody";

storiesOf("PersonaBarPage", module).add("with content", () => <PersonaBarPage isOpen={true} className="">
    <PersonaBarPageHeader title="Page Header">
    </PersonaBarPageHeader>
    <PersonaBarPageBody backToLinkProps={{
        text: "Back",
        onClick: { }
    }}>
        <div>Page Content</div>
    </PersonaBarPageBody>
</PersonaBarPage>);