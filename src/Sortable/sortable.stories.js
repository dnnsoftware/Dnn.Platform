import React, { Component } from "react";
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

storiesOf("Sortable", module).add("in rows", () => (
    <Sortable
        onSort={action("Sorted")}
        items={testProperties}
        sortOnDrag={true}>
            {renderRows()}
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

export default class MyComponent extends Component{
    render(){
        return(
            <div className="my-component">
                <span>{this.props.name}</span>
            </div>
        );
    }
}