import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import BackToLink from "../BackToLink";
import Button from "../Button";
import Checkbox from "../Checkbox";
import Collapsible from "../Collapsible";
import CollapsibleRow from "../CollapsibleRow";
import ContentLoadWrapper from "../ContentLoadWrapper";
import DatePicker from "../DatePicker";
import DayPicker from "../DayPicker";
import Dropdown from "../Dropdown";
import DropdownWithError from "../DropdownWithError";
import EditableField from "../EditableField";
//import FileUpload from "../FileUpload";
import Flag from "../Flag";
//import FolderPicker from "../FolderPicker";
// GridCell
import InputGroup from "../InputGroup";
import Label from "../Label";
import Modal from "../Modal";
import MultiLineInputWithError from "../MultiLineInputWithError";
import NumberSlider from "../NumberSlider";


import SingleLineInputWithError from "../SingleLineInputWithError";


import { ArrowDownIcon, TableEmptyState } from "../SvgIcons";

storiesOf("BackToLink", module)
    .add("with text", () => <BackToLink onClick={action("clicked")} text="Hello BackToLink"></BackToLink>); 

storiesOf("Button", module)
    .add("with text", () => <Button onClick={action("clicked")}>Hello Button</Button>); 
    
storiesOf("Checkbox", module)
    .add("with text", () => <Checkbox value={false} onChange={action("changed")} label="Hello Checkbox"></Checkbox>); 

storiesOf("Collapsible", module)
    .add("with text", () => <Collapsible isOpened={true} autoScroll={true} scrollDelay={10}>
        <div>
            <p className="add-term-title">Add Term</p>
            <InputGroup>
                <SingleLineInputWithError
                    inputId={"create-term-name"}
                    withLabel={true}
                    label="Required Term *"
                    value="Term Value"
                    onChange={action("changed")}
                    errorMessage="Error"
                />
            </InputGroup>
            <InputGroup>
                <MultiLineInputWithError
                    inputId={"create-term-description"}
                    withLabel={true}
                    label="Description"
                    value="Long descritpion here"
                    onChange={action("changed")}/>
            </InputGroup>
        </div>
    </Collapsible>); 

storiesOf("CollapsibleRow", module)
    .add("with text", () => <CollapsibleRow label={<div>Click Header To Expand</div>}
        keepCollapsedContent={true}
        closeOnBlur={false}
        secondaryButtonText="Close"
        buttonsAreHidden={false}
        collapsed={true}
        onChange={action("changed")}><p>Test Content</p></CollapsibleRow>); 

storiesOf("ContentLoadWrapper", module)
    .add("with loading", () =>  
        <ContentLoadWrapper loadComplete={false}
            svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}>
            <div>
                <div className="auditcheck-topbar">
                    Loading...
                </div>
                <div className="auditCheckItems">
                    <ul>
                        <li>Item 1</li>
                        <li>Item 2</li>
                        <li>Item 3</li>
                    </ul>
                </div>
            </div>
        </ContentLoadWrapper>
    );

storiesOf("ContentLoadWrapper", module)
    .add("with content", () =>  
        <ContentLoadWrapper loadComplete={true}
            svgSkeleton={<div dangerouslySetInnerHTML={{ __html: TableEmptyState }} />}>
            <div>
                <div className="auditcheck-topbar">
                    <h1>Content</h1>
                </div>
                
                <div className="auditCheckItems">
                    <ul>
                        <li>Item 1</li>
                        <li>Item 2</li>
                        <li>Item 3</li>
                    </ul>
                </div>
            </div>
        </ContentLoadWrapper>
    );

let startDate = new Date("December 17, 2018 03:24:00");
storiesOf("DatePicker", module)
    .add("with content", () =>  
        <div className="scheduler-date-row">
            <Label
                label="Start Date"/>
            <DatePicker
                date={startDate}
                updateDate={(date) => action("changed " + date.toString()) }
                isDateRange={false}
                hasTimePicker={true}
                showClearDateButton={false} />
        </div>);

storiesOf("DayPicker", module)
    .add("with content", () => <DayPicker />); 

storiesOf("Dropdown", module)
    .add("with content", () => <Dropdown label="Test" options={[{ label: "Opt 1", value: 1 }, { label: "Opt 2", value: 2 }, { label: "Opt 3", value: 3 }]} selectedIndex={1}></Dropdown>); 
  
storiesOf("DropdownWithError", module)
    .add("with content", () => <DropdownWithError error={true} errorMessage="Please select an item" label="Test" options={[{ label: "Opt 1", value: 1 }, { label: "Opt 2", value: 2 }, { label: "Opt 3", value: 3 }]}></DropdownWithError>); 

storiesOf("EditableField", module)
    .add("with content", () => <EditableField label="Test" value="Content" onFocus={action("focus")} onEnter={action("enter")} />);

// TODO: Need FileUpload example here. It is complicated and most of the dependent required utils live in the Dnn.Admin.Experience project.

storiesOf("Flag", module)
    .add("with content", () => <Flag title="Test" culture="en-US" onClick={action("Clicked")} />);

// TODO: Need FolderPicker example here. Not sure what to put in for the serviceFramework prop

storiesOf("InputGroup", module)
    .add("with content", () =>
        <InputGroup>
            <SingleLineInputWithError
                inputId={"create-term-name"}
                withLabel={true}
                label="Required Term *"
                value="Term Value"
                onChange={action("changed")}
                errorMessage="Error"
            />
        </InputGroup>
    );

storiesOf("Label", module)
    .add("with content", () => <Label label="Test" />);

storiesOf("Modal", module)
    .add("with content", () => <Modal isOpen={true} header="Test Modal"><div>Hello!</div></Modal>);

storiesOf("MultiLineInputWithError", module)
    .add("with content", () => <MultiLineInputWithError
        inputId={"create-term-description"}
        withLabel={true}
        label="Description"
        value="Long descritpion here"
        onChange={action("changed")}/>);

storiesOf("NumberSlider", module)
    .add("with content", () => <NumberSlider min={1} max={100} step={5} value={50} onChange={action("Change")} />);
    
storiesOf("SingleLineInputWithError", module)
    .add("with content", () =>     <SingleLineInputWithError
        inputId={"create-term-name"}
        withLabel={true}
        label="Required Term *"
        value="Term Value"
        onChange={action("changed")}
        errorMessage="Error"
    />
    );