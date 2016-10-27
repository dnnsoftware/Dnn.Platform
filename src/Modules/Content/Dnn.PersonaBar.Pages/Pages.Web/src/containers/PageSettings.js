import React, {Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import Localization from "../localization";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import PermissionGrid from "../../node_modules/dnn-permission-grid";
import utils from "../utils";
import cloneDeep from "lodash/cloneDeep";

class PageSettings extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    getButtons() {
        const {onCancel, onSave} = this.props;

        return (
            <div className="buttons-box">
                <Button
                    type="secondary"
                    onClick={onCancel}>
                    {Localization.get("Cancel")}
                </Button>
                <Button
                    type="primary"
                    onClick={onSave}>
                    {Localization.get("Save")}
                </Button>
            </div>
        );
    }

    render() {
        const {selectedPage} = this.props;
        const buttons = this.getButtons();

        return (
            <Tabs tabHeaders={[Localization.get("Details"), Localization.get("Permissions")]}>
                <div>
                    <GridSystem>
                        <SingleLineInputWithError
                            label={Localization.get("Name")}
                            tooltipMessage={Localization.get("page_name_tooltip")}
                            value={selectedPage.name} 
                            onChange={this.onChangeField.bind(this, "name")} />
                        <SingleLineInputWithError
                            label={Localization.get("Title")}
                            tooltipMessage={Localization.get("page_title_tooltip")}
                            value={selectedPage.title}
                            onChange={this.onChangeField.bind(this, "title")} />
                    </GridSystem>
                    <GridCell>
                        <MultiLineInputWithError
                            label={Localization.get("Description")}
                            value={selectedPage.description}
                            onChange={this.onChangeField.bind(this, "description")} />
                    </GridCell>
                    <GridCell>
                        <MultiLineInputWithError
                            label={Localization.get("Keywords")}
                            value={selectedPage.keywords} 
                            onChange={this.onChangeField.bind(this, "keywords")} />
                    </GridCell>
                    <GridCell>
                        <SingleLineInputWithError
                            label={Localization.get("Tags")}
                            value={selectedPage.tags} 
                            onChange={this.onChangeField.bind(this, "tags")} />
                        <SingleLineInputWithError
                            label={Localization.get("URL")}
                            value={selectedPage.url}
                            enabled={false} />
                    </GridCell>
                    {buttons}
                </div>
                <div>                
                    <PermissionGrid 
                        permissions={cloneDeep(selectedPage.permissions)} 
                        service={utils.utilities.sf} 
                        onPermissionsChanged={this.props.onPermissionsChanged} />                
                    {buttons}
                </div>
            </Tabs>
        );
    }    
}

PageSettings.propTypes = {
    selectedPage: PropTypes.object.isRequired,
    onCancel: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onChangeField: PropTypes.func.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired
};

export default PageSettings;

