import React, {Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import Localization from "../../localization";
import PageDetails from "../PageDetails/PageDetails";
import PermissionGrid from "../../../node_modules/dnn-permission-grid";
import utils from "../../utils";
import Button from "dnn-button";
import styles from "./style.less";
import Modules from "../Modules/Modules";


class PageSettings extends Component {

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
        const {selectedPage, onChangeField} = this.props;
        const buttons = this.getButtons();

        return (
            <Tabs 
                tabHeaders={[Localization.get("Details"), 
                             Localization.get("Permissions"), 
                             Localization.get("Advanced")]}
                className={styles.pageSettings}>
                <div className="dnn-simple-tab-item">
                    <PageDetails 
                        page={selectedPage} 
                        onChangeField={onChangeField} />
                    {buttons}
                </div>
                <div className="dnn-simple-tab-item">                
                    <PermissionGrid 
                        permissions={selectedPage.permissions} 
                        service={utils.utilities.sf} 
                        onPermissionsChanged={this.props.onPermissionsChanged} />                
                    {buttons}
                </div>
                <div>
                    <Tabs 
                        tabHeaders={[Localization.get("Modules"), 
                                     Localization.get("Appearance"), 
                                     Localization.get("S.E.O."), 
                                     Localization.get("More")]}
                        
                        type="secondary">
                        <div className="dnn-simple-tab-item">
                            <Modules modules={selectedPage.modules}/>
                        </div>
                        <div></div>
                        <div></div>
                        <div></div>
                    </Tabs>
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

