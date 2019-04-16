import React, {Component} from "react";
import PropTypes from "prop-types";
import { Button, MultiLineInput, Label } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import styles from "./style.less";
import BranchParent from "./BranchParent";
import KeyWords from "./KeyWords";
import PageTags from "./Tags";
import DisplayInMenu from "./DisplayInMenu";
import EnableScheduling from "./EnableScheduling";

class AddPages extends Component {
    constructor(props){
        super(props);
        this._isFinishLoad = false;

    }

    onChangeValue(key, value) {
        const {onChangeField} = this.props;
        onChangeField(key, value);
    }

    onChangeEvent(key, event) {
        this.onChangeValue(key, event.target.value);
    }

    onChangeTags(tags) {
        this.onChangeValue("tags", tags.join(","));
    }

    getLeftColumnComponents() {
        const {bulkPage} = this.props;
        const tags = bulkPage.tags ? bulkPage.tags.split(",") : [];

        const defaultLeftColumnComponents = [
            <BranchParent key="branchParent" parentId={bulkPage.parentId}
                onChangeValue={this.onChangeValue.bind(this)} />,
            <KeyWords key="keywords" keywords={bulkPage.keywords}
                onChangeEvent={this.onChangeEvent.bind(this)} />,
            <PageTags key="tags" tags={tags}
                onChangeTags={this.onChangeTags.bind(this)} />,
            <DisplayInMenu key="displayInMenu" includeInMenu={bulkPage.includeInMenu}
                onChangeValue={this.onChangeValue.bind(this)} />,
            <EnableScheduling key="enableScheduling" startDate={bulkPage.startDate}
                endDate={bulkPage.endDate}
                onChangeValue={this.onChangeValue.bind(this)} />
        ];
        
        const additionalComponents = this.props.components;

        this.insertElementsInArray(defaultLeftColumnComponents, additionalComponents);
        this._isFinishLoad = true;
        return defaultLeftColumnComponents;
    }

    insertElementsInArray(array, elements) {
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i].order);
            const Component = elements[i].component;
            const instance = <div className="input-group">
                <Component bulkPage={this.props.bulkPage} onChange={this.onChangeValue.bind(this)} 
                    store={elements[i].store} /></div>;
         
            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, instance);
            }            
        }
    }

    getInteger(value) {
        if (value === 0) {
            return 0;
        }
        if (value) {
            return parseInt(value.toString());
        }
        return value;
    } 
    
 
    render() {
        const {bulkPage, onCancel, onValidate, onSave} = this.props;

        return (
            <div className={styles.addPages}>
                <div className="addPagesDetail">
                    <div className="grid-columns">
                        <div className="left-column">
                            <div className="column-heading">
                                {Localization.get("BulkPageSettings")}
                            </div>
                            {this.getLeftColumnComponents()}
                        </div>
                        <div className="right-column">
                            <div className="column-heading">
                                {Localization.get("BulkPagesToAdd")}
                            </div>
                            {/* white space no wrap to keep this label in single line */}
                            <Label label={Localization.get("BulkPagesLabel")} style={{"white-space":"nowrap"}} />
                            <MultiLineInput
                                onChange={(event) => this.onChangeEvent("bulkPages", event)}
                                value={bulkPage.bulkPages}
                                className="bulk-page-input" />
                        </div>
                    </div>
                    <div style={{clear: "both"}}></div>
                    <div className="buttons-box">
                        <Button
                            type="secondary"
                            onClick={onCancel}>
                            {Localization.get("Cancel")}
                        </Button>
                        <Button
                            type="secondary"
                            onClick={onValidate}
                            disabled={!bulkPage.bulkPages}>
                            {Localization.get("ValidatePages")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={onSave}
                            disabled={!bulkPage.bulkPages}>
                            {Localization.get("AddPages")}
                        </Button>
                    </div>
                </div>
            </div>
        );
    }
}

AddPages.propTypes = {
    bulkPage: PropTypes.object.isRequired,
    onCancel: PropTypes.func.isRequired,
    onValidate: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onChangeField: PropTypes.func.isRequired, 
    components: PropTypes.array.isRequired
};

export default AddPages;