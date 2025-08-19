import React, { Component } from "react";
import { action } from "storybook/actions";
import Sortable from "./index";
import Label from "../Label";

export default {
    component: Sortable,
};

export const WithContent = () => (
    <Sortable
        onSort={action("Sorted")}
        items={testProperties}
        sortOnDrag={true}>
        {renderedProperties() }
    </Sortable>
);

export const InRows = () => (
    <Sortable
        onSort={action("Sorted")}
        items={testProperties}
        sortOnDrag={true}
    >
        {renderRows()}
    </Sortable>
);

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

function renderRows() {
    let i=0;
    return testProperties.map((item, index) => {
        return (
            <MyComponent
                key={"row-" + index}
                id={item.id}
                name={item.name}
            ></MyComponent>
        );
    });
}