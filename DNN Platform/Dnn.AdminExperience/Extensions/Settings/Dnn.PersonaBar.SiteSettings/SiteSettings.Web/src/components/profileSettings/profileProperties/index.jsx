import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../../actions";
import ProfilePropertyRow from "./profilePropertyRow";
import ProfilePropertyEditor from "./profilePropertyEditor";
import "./style.less";
import util from "../../../utils";
import resx from "../../../resources";
import { Sortable, Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";

let tableFields = [];


class ProfilePropertiesPanel extends Component {
    constructor() {
        super();
        this.state = {
            openId: ""
        };
    }

    loadData() {
        const {props} = this;
        if (props.profileProperties) {
            if (props.portalId === undefined || props.profileProperties.PortalId === props.portalId) {
                return false;
            }
            else {
                return true;
            }
        }
        else {
            return true;
        }
    }

    componentDidMount() {
        const {props} = this;

        if (!this.loadData()) {
            return;
        }
        else {
            props.dispatch(SiteBehaviorActions.getProfileProperties(props.portalId, (data) => {
                this.setState({
                    pid: data.PortalId
                });
            }));
        }

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
        this.setState({
            openId: id
        });
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        const {props} = this;
        if (openId !== "") {
            if (props.profilePropertyClientModified) {
                util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                    props.dispatch(SiteBehaviorActions.cancelProfilePropertyClientModified());
                    this.uncollapse(openId);
                });
            }
            else {
                this.uncollapse(openId);
            }
        }
    }

    onDeleteProperty(propertyId) {
        const {props} = this;

        if (props.profilePropertyClientModified) {
            util.utilities.notifyError(resx.get("SaveOrCancelWarning"));
        }
        else {
            util.utilities.confirm(resx.get("PropertyDefinitionDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
                const profileProperties = Object.assign({}, props.profileProperties);
                profileProperties.Properties = profileProperties.Properties.filter((item) => item.PropertyDefinitionId !== propertyId);
                props.dispatch(SiteBehaviorActions.deleteProfileProperty(propertyId, profileProperties, () => {
                    util.utilities.notify(resx.get("DeleteSuccess"));
                    this.collapse();
                }, () => {
                    util.utilities.notify(resx.get("DeleteError"));
                }));
            });
        }
    }

    findWithAttr(array, attr, value) {
        if (array) {
            for (let i = 0; i < array.length; i += 1) {
                if (array[i][attr] === value) {
                    return i;
                }
            }
        }
        return -1;
    }

    onMovePropertyUp(propertyId) {
        const {props} = this;

        if (props.profilePropertyClientModified) {
            util.utilities.notifyError(resx.get("SaveOrCancelWarning"));
        }
        else {
            let profileProperties = Object.assign({}, props.profileProperties);
            let items = Object.assign([], props.profileProperties.Properties);
            let index = this.findWithAttr(items, "PropertyDefinitionId", propertyId);

            if (index > 0) {
                let tmp = items[index];
                items[index] = items[index - 1];
                items[index - 1] = tmp;
                profileProperties.Properties = items;
                props.dispatch(SiteBehaviorActions.updateProfilePropertyOrders(
                    {
                        PortalId: props.portalId,
                        Properties: items
                    }, profileProperties, () => {
                        util.utilities.notify(resx.get("ViewOrderUpdateSuccess"));
                        this.collapse();
                    }, (error) => {
                        const errorMessage = JSON.parse(error.responseText);
                        util.utilities.notifyError(errorMessage.Message);
                    }));
            }
        }
    }

    onMovePropertyDown(propertyId) {
        const {props} = this;

        if (props.profilePropertyClientModified) {
            util.utilities.notifyError(resx.get("SaveOrCancelWarning"));
        }
        else {
            let profileProperties = Object.assign({}, props.profileProperties);
            let items = Object.assign([], props.profileProperties.Properties);
            let index = this.findWithAttr(items, "PropertyDefinitionId", propertyId);

            if (index < items.length - 1) {
                let tmp = items[index];
                items[index] = items[index + 1];
                items[index + 1] = tmp;
                profileProperties.Properties = items;
                props.dispatch(SiteBehaviorActions.updateProfilePropertyOrders(
                    {
                        PortalId: props.portalId,
                        Properties: items
                    }, profileProperties, () => {
                        util.utilities.notify(resx.get("ViewOrderUpdateSuccess"));
                        this.collapse();
                    }, (error) => {
                        const errorMessage = JSON.parse(error.responseText);
                        util.utilities.notifyError(errorMessage.Message);
                    }));
            }
        }
    }

    onSort(items) {
        const {props} = this;
        let profileProperties = Object.assign({}, props.profileProperties);
        profileProperties.Properties = items;
        //props.dispatch(SiteBehaviorActions.sortProfileProperty(profileProperties));
        props.dispatch(SiteBehaviorActions.updateProfilePropertyOrders(
            {
                PortalId: props.portalId,
                Properties: items
            }, profileProperties, () => {
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
    }

    /* eslint-disable react/no-danger */
    renderedProfileProperties() {
        let i = 0;
        if (this.props.profileProperties) {
            return this.props.profileProperties.Properties.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <ProfilePropertyRow
                        propertyId={item.PropertyDefinitionId}
                        name={item.PropertyName}
                        dataType={item.DataType}
                        defaultVisibility={resx.get(item.DefaultVisibility) }
                        required={item.Required}
                        visible={item.Visible}
                        index={index}
                        key={"propertyItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this) }
                        Collapse={this.collapse.bind(this) }
                        onDelete={this.onDeleteProperty.bind(this, item.PropertyDefinitionId) }
                        onMoveUp={this.onMovePropertyUp.bind(this, item.PropertyDefinitionId) }
                        onMoveDown={this.onMovePropertyDown.bind(this, item.PropertyDefinitionId) }
                        id={id}>
                        <ProfilePropertyEditor
                            portalId={this.props.portalId}
                            cultureCode={this.props.cultureCode}
                            propertyId={item.PropertyDefinitionId}
                            Collapse={this.collapse.bind(this) }
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
                        <div className="sectionTitle">{resx.get("UserProfileFields") }</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add") }>
                            <div className="add-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }}>
                            </div> {resx.get("cmdAddField") }
                        </div>
                    </div>
                    <div className="property-items-grid">
                        {this.renderHeader() }
                        <Collapsible isOpened={opened} autoScroll={true} style={{ overflow: opened ? "visible" : "hidden", width: "100%" }}>
                            <ProfilePropertyRow
                                name={"-"}
                                dataType={"-"}
                                defaultVisibility={"-"}
                                index={"add"}
                                key={"propertyItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this) }
                                Collapse={this.collapse.bind(this) }
                                onDelete={this.onDeleteProperty.bind(this) }
                                onMoveUp={this.onMovePropertyUp.bind(this) }
                                onMoveDown={this.onMovePropertyDown.bind(this) }
                                id={"add"}>
                                <ProfilePropertyEditor
                                    portalId={this.props.portalId}
                                    cultureCode={this.props.cultureCode}
                                    Collapse={this.collapse.bind(this) }
                                    id={"add"}
                                    openId={this.state.openId} />
                            </ProfilePropertyRow>
                        </Collapsible>
                        {this.props.profileProperties && this.state.openId &&
                            this.renderedProfileProperties()
                        }
                        {this.props.profileProperties && !this.state.openId &&
                            <Sortable
                                onSort={this.onSort.bind(this) }
                                items={this.props.profileProperties.Properties}
                                sortOnDrag={true}>
                                {this.renderedProfileProperties() }
                            </Sortable>
                        }
                    </div>
                </div>

            </div >
        );
    }
}
// sortOnDrag={true}

ProfilePropertiesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    profileProperties: PropTypes.object,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string,
    profilePropertyClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        profileProperties: state.siteBehavior.profileProperties,
        tabIndex: state.pagination.tabIndex,
        profilePropertyClientModified: state.siteBehavior.profilePropertyClientModified
    };
}

export default connect(mapStateToProps)(ProfilePropertiesPanel);