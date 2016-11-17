import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    pagination as PaginationActions,
    siteBehavior as SiteBehaviorActions
} from "../../../actions";
import ProfilePropertyRow from "./profilePropertyRow";
import ProfilePropertyEditor from "./profilePropertyEditor";
import Collapse from "react-collapse";
import Select from "dnn-select";
import "./style.less";
import { AddIcon } from "dnn-svg-icons";
import util from "../../../utils";
import resx from "../../../resources";

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
        props.dispatch(SiteBehaviorActions.getProfileProperties(props.portalId));

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

    onUpdateProperty(propertyDetail) {
        const {props, state} = this;
        if (propertyDetail.DefinitionId) {
            props.dispatch(SiteBehaviorActions.UpdateProfileProperty(propertyDetail, (data) => {
                util.utilities.notify(resx.get("PropertyDefinitionUpdateSuccess"));
                this.collapse();
                props.dispatch(SiteBehaviorActions.getProfileProperties(props.portalId));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SiteBehaviorActions.AddProfileProperty(propertyDetail, (data) => {
                util.utilities.notify(resx.get("PropertyDefinitionCreateSuccess"));
                this.collapse();
                props.dispatch(SiteBehaviorActions.getProfileProperties(props.portalId));
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
            props.dispatch(SiteBehaviorActions.deleteProfileProperty(propertyId, itemList, () => {
                util.utilities.notify(resx.get("DeleteSuccess"));
                this.collapse();
            }, (error) => {
                util.utilities.notify(resx.get("DeleteError"));
            }));
        });
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
        const {props, state} = this;

        if (props.profilePropertyClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelProfilePropertyClientModified());
                this.moveUp(propertyId);
            }, () => {
                return;
            });
        }
        else {
            this.moveUp(propertyId);
        }
    }

    moveUp(propertyId) {
        const {props} = this;
        const itemList = Object.assign([], props.profileProperties);
        let index = this.findWithAttr(itemList, "PropertyDefinitionId", propertyId);

        if (index > 0) {
            let tmp = itemList[index];
            itemList[index] = itemList[index - 1];
            itemList[index - 1] = tmp;
            props.dispatch(SiteBehaviorActions.swapProfilePropertyOrders(
                {
                    PortalId: props.portalId,
                    FirstPropertyDefinitionId: propertyId,
                    SecondPropertyDefinitionId: itemList[index].PropertyDefinitionId
                }, itemList, () => {
                    util.utilities.notify(resx.get("ViewOrderUpdateSuccess"));
                    this.collapse();
                }, (error) => {
                    const errorMessage = JSON.parse(error.responseText);
                    util.utilities.notifyError(errorMessage.Message);
                }));
        }
    }

    onMovePropertyDown(propertyId) {
        const {props, state} = this;

        if (props.profilePropertyClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelProfilePropertyClientModified());
                this.moveDown(propertyId);
            }, () => {
                return;
            });
        }
        else {
            this.moveDown(propertyId);
        }
    }

    moveDown(propertyId) {
        const {props} = this;

        const itemList = Object.assign([], props.profileProperties);
        let index = this.findWithAttr(itemList, "PropertyDefinitionId", propertyId);

        if (index < itemList.length - 1) {
            let tmp = itemList[index];
            itemList[index] = itemList[index + 1];
            itemList[index + 1] = tmp;
            props.dispatch(SiteBehaviorActions.swapProfilePropertyOrders(
                {
                    PortalId: props.portalId,
                    FirstPropertyDefinitionId: propertyId,
                    SecondPropertyDefinitionId: itemList[index].PropertyDefinitionId
                }, itemList, () => {
                    util.utilities.notify(resx.get("ViewOrderUpdateSuccess"));
                    this.collapse();
                }, (error) => {
                    const errorMessage = JSON.parse(error.responseText);
                    util.utilities.notifyError(errorMessage.Message);
                }));
        }
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
                        onDelete={this.onDeleteProperty.bind(this, item.PropertyDefinitionId)}
                        onMoveUp={this.onMovePropertyUp.bind(this, item.PropertyDefinitionId)}
                        onMoveDown={this.onMovePropertyDown.bind(this, item.PropertyDefinitionId)}
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
                            </div> {resx.get("cmdAddField")}
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
                                onMoveUp={this.onMovePropertyUp.bind(this)}
                                onMoveDown={this.onMovePropertyDown.bind(this)}
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
    profileProperties: PropTypes.array,
    portalId: PropTypes.number,
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