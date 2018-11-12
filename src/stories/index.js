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
//import GridCell from "../GridCell";
import InputGroup from "../InputGroup";
import Label from "../Label";
import Modal from "../Modal";
import MultiLineInputWithError from "../MultiLineInputWithError";
import NumberSlider from "../NumberSlider";
//import Pager from "../Pager";
//import PermissionGrid from "../PermissionGrid";
//import PersonaBarPage from "../PersonaBarPage";
//import PersonaBarPageBody from "../PersonaBarPageBody";
//import PersonaBarPageHeader from "../PersonaBarPageHeader";
//import PortableTransitionModal from "../PortableTransitionModal";
import RadioButtons from "../RadioButtons";
import ScrollBar from "../ScrollBar";
//import SearchableTags from "../SearchableTags";
//import SearchBox from "../SearchBox";
//import Select from "../Select";
import SingleLineInputWithError from "../SingleLineInputWithError";
//import Sortable from "../Sortable";
//import Switch from "../Switch";
//import Tags from "../Tags";
//import TextOverflowWrapperNew from "../TextOverflowWrapperNew";
//import Tooltip from "../Tooltip";
//import TransitionModal from "../TransitionModal";
//import TreeControlInteractor from "../TreeControlInteractor";

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
    .add("with single line input", () => <EditableField label="Test" value="Content" onFocus={action("focus")} onEnter={action("enter")} />);

storiesOf("EditableField", module)
    .add("with multi-line input", () => <EditableField label="Test" value="Content" inputType="textArea" onFocus={action("focus")} onEnter={action("enter")} />);

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

//storiesOf("Modal", module)
//    .add("with content", () => <Modal isOpen={true} header="Test Modal"><div>Hello!</div></Modal>);

storiesOf("MultiLineInputWithError", module)
    .add("with content", () => <MultiLineInputWithError
        inputId={"create-term-description"}
        withLabel={true}
        label="Description"
        value="Long descritpion here"
        onChange={action("changed")}/>);

storiesOf("NumberSlider", module)
    .add("with content", () => <NumberSlider min={1} max={100} step={5} value={50} onChange={action("Change")} />);

// TODO: Need Pager example
// TODO: Need PermissionGrid example
// TODO: Need PersonaBarPage example
// TODO: Need PersonaBarPageBody example
// TODO: Need PersonaBarPageHeader example
// TODO: Need PortableTransitionModal example
storiesOf("RadioButtons", module)
    .add("with content", () =>  <RadioButtons
        options={[{ value: "1", label: "Value 1" }, { value: "2", label: "Value 2" }]}
        onChange={action("Change")}
        value={"1"} />);

storiesOf("ScrollBar", module)
    .add("with content", () =>  <ScrollBar>
        <div>
            <p>
          Condimentum faucibus fermentum ut habitant phasellus lacus natoque curae; tempor eget interdum. Arcu lacus lobortis augue fames torquent nunc blandit eros. Sollicitudin etiam suscipit ante condimentum malesuada posuere laoreet phasellus? Enim dignissim praesent posuere primis laoreet conubia suscipit nec hac. Aliquam class sem primis lacinia risus montes suspendisse nec tempor? Natoque, est mi quam donec ultricies faucibus. Aliquam leo sodales praesent luctus mauris vivamus purus senectus risus dis risus duis. Dis risus imperdiet litora arcu maecenas porta class. Taciti montes elit donec nibh. Vitae facilisis ornare phasellus vel nostra. Mi duis molestie.
            </p>
            <p>
          Molestie praesent aliquam metus et etiam vitae orci et. Platea tempus ullamcorper venenatis lacinia nam tincidunt tristique lacus magnis massa sit. Ornare vivamus natoque, inceptos consequat. Suscipit potenti aenean ultrices. Dis eros imperdiet ornare montes urna lacus! Tortor dignissim lectus molestie viverra nec dapibus aliquet natoque consectetur vivamus dui. Condimentum erat auctor sollicitudin purus lorem! Suspendisse sodales odio ultricies dignissim! Porta convallis venenatis, orci faucibus leo bibendum cubilia natoque magnis. Pretium suspendisse natoque sapien aliquam semper magna class mattis. At.
            </p>
            <p>
          Viverra sodales torquent magna justo inceptos convallis nec. Cras scelerisque interdum sapien? Fusce a felis dis aptent. Lobortis lobortis, urna quisque urna. Sagittis per penatibus duis. Elementum sed aenean feugiat. Sodales rutrum lectus accumsan nisl lorem! Taciti a erat consequat fermentum ornare commodo conubia consequat blandit inceptos odio vehicula! Lacinia dictum phasellus pellentesque suspendisse convallis lacinia eleifend sociis. Ultrices interdum mauris interdum, tellus praesent praesent ac phasellus dictumst lacinia ultrices consequat. Cras mi nostra nostra ligula? Viverra nunc.
            </p>
            <p>
          Pretium sollicitudin ac nec feugiat augue mollis mattis ridiculus imperdiet turpis. Diam enim sit iaculis pharetra, laoreet potenti lobortis pharetra. Vehicula nostra aliquam interdum euismod potenti bibendum rutrum per id eu. Vivamus hendrerit pharetra rhoncus lacinia ad laoreet eros condimentum consectetur vestibulum cubilia tellus. Vulputate volutpat ut eleifend tincidunt mollis sodales mauris cubilia parturient cras sit sodales. Tellus ultrices odio ut sed odio eget molestie. Elementum ipsum laoreet quis netus magnis morbi vel quisque mattis venenatis. Dictumst bibendum semper praesent hac nec ullamcorper netus per commodo! Eget in sociis orci iaculis dictumst.
            </p>
            <p>
          Etiam, lobortis quis sagittis. Nullam parturient habitant rhoncus sollicitudin netus. Augue arcu ultricies condimentum id eleifend sapien dictumst quam integer? Ultrices hendrerit placerat accumsan. Sodales arcu semper ultricies sit vivamus cras! Scelerisque, inceptos blandit sem mi sed ornare senectus lacus semper. Ornare bibendum sed iaculis praesent eu sed scelerisque elementum pulvinar lacinia erat pulvinar. In tincidunt donec elementum natoque vehicula molestie scelerisque ultricies erat, cubilia gravida. Congue platea tincidunt magnis vehicula massa dictumst ac dolor vulputate potenti habitant.
            </p>    
        </div>
    </ScrollBar>);
// TODO: Need SearchBox example
// TODO: Need Select example

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

// TODO: Need Sortable example
// TODO: Need Switch example
// TODO: Need Tags example
// TODO: Need TextOverflowWrapperNew example
// TODO: Need Tooltip example
// TODO: Need TransitionModal example
// TODO: Need TreeControlInteractor example
