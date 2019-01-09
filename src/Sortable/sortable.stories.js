import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Sortable from "./index";
import Label from "../Label";

storiesOf("Sortable", module).add("with content", () => (
    <Sortable
        onSort={action("Sorted")}
        items={testProperties}
        sortOnDrag={true}>
        {renderedProperties() }
    </Sortable>
));

const testProperties =
    [
        {
            id: 1,
            index: 0,
            name: "Item 1"
        },
        {
            id: 2,
            index: 1,
            name: "Item 2"
        },
        {
            id: 3,
            index: 2,
            name: "Item 3"
        }
    ];

function renderedProperties() {
    let i = 0;
    return testProperties.map((item, index) => {
        let id = "row-" + i++;
        return (
            <Label
                key={id}
                id={item.id}
                name={item.name} label={item.name} />
        );
    });
}