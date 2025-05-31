import React from "react";
import PersonaBarPage from "./index";
import PersonaBarPageHeader from "../PersonaBarPageHeader";
import PersonaBarPageBody from "../PersonaBarPageBody";

export const WithContent = () => (
    <PersonaBarPage isOpen={true} className="">
        <PersonaBarPageHeader title="Page Header">
        </PersonaBarPageHeader>
        <PersonaBarPageBody backToLinkProps={{
            text: "Back",
            onClick: { }
        }}>
            <div>Page Content</div>
        </PersonaBarPageBody>
    </PersonaBarPage>
);