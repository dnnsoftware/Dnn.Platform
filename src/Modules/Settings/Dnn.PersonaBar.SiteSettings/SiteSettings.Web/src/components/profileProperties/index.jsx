import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    siteSettings as SiteSettingsActions
} from "../../actions";
import ProfilePropertyRow from "./profilePropertyRow";
import ProfilePropertyEditor from "./profilePropertyEditor";
import Collapse from "react-collapse";
import Select from "dnn-select";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../utils";
import resx from "../../resources";

let tableFields = [];

class ProfilePropertiesPanel extends Component {
    constructor() {
        super();
        this.state = {
            profileProperties: [],
            openId: ""
        };
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(SiteSettingsActions.getProfileProperties());

        tableFields = [];
        tableFields.push({ "name": resx.get("Name.Header"), "id": "Name" });
        tableFields.push({ "name": resx.get("DataType.Header"), "id": "DataType" });
        tableFields.push({ "name": resx.get("DefaultVisibility.Header"), "id": "DefaultVisibility" });
        tableFields.push({ "name": resx.get("Required.Header"), "id": "Required" });
        tableFields.push({ "name": resx.get("Visible.Header"), "id": "Visible" });
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "property-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateProperty(propertyDetail) {
        const {props, state} = this;
        if (propertyDetail.DefinitionId) {
            props.dispatch(SiteSettingsActions.UpdateProfileProperty(propertyDetail, (data) => {
                util.utilities.notify(resx.get("PropertyDefinitionUpdateSuccess"));
                this.collapse();
                props.dispatch(SiteSettingsActions.getProfileProperties());
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SiteSettingsActions.AddProfileProperty(propertyDetail, (data) => {
                util.utilities.notify(resx.get("PropertyDefinitionCreateSuccess"));
                this.collapse();
                props.dispatch(SiteSettingsActions.getProfileProperties());
            }, (error) => {
                util.utilities.notify(resx.get("PropertyDefinitionCreateError"));
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteProperty(propertyId) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("PropertyDefinitionDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const itemList = props.profileProperties.filter((item) => item.PropertyDefinitionId !== propertyId);
            props.dispatch(SiteSettingsActions.deleteProfileProperty({ "propertyId": propertyId }, itemList, () => {
                util.utilities.notify(resx.get("DeleteSuccess"));
                this.collapse();
            }, (error) => {
                util.utilities.notify(resx.get("DeleteError"));
            }));
        });
    }

    /* eslint-disable react/no-danger */
    renderedProfileProperties() {
        let i = 0;
        if (this.props.profileProperties) {
            return this.props.profileProperties.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <ProfilePropertyRow
                        propertyId={item.PropertyDefinitionId}
                        name={item.PropertyName}
                        dataType={item.DataType}
                        defaultVisibility={resx.get(item.DefaultVisibility)}
                        required={item.Required}
                        visible={item.Visible}
                        index={index}
                        key={"propertyItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}
                        onDelete={this.onDeleteProperty.bind(this)}
                        id={id}>
                        <ProfilePropertyEditor
                            propertyId={item.PropertyDefinitionId}
                            Collapse={this.collapse.bind(this)}                            
                            onUpdate={this.onUpdateProperty.bind(this)}
                            id={id}
                            openId={this.state.openId} />
                    </ProfilePropertyRow>
                );
            });
        }
    }

    render() {
        let opened = (this.state.openId === "add");
        return (
            <div>
                <div className="property-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">{resx.get("UserProfileFields")}</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: AddIcon }}>
                            </div> Add Field
                        </div>
                    </div>
                    <div className="property-items-grid">
                        {this.renderHeader()}
                        <Collapse isOpened={opened} style={{ float: "left", width: "100%" }}>
                            <ProfilePropertyRow
                                name={"-"}
                                dataType={"-"}
                                defaultVisibility={"-"}
                                index={"add"}
                                key={"propertyItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteProperty.bind(this)}
                                id={"add"}>
                                <ProfilePropertyEditor
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateProperty.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </ProfilePropertyRow>
                        </Collapse>
                        {this.renderedProfileProperties()}
                    </div>
                </div>

            </div >
        );
    }
}

ProfilePropertiesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    profileProperties: PropTypes.array
};

function mapStateToProps(state) {
    return {
        profileProperties: state.siteSettings.profileProperties,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(ProfilePropertiesPanel);