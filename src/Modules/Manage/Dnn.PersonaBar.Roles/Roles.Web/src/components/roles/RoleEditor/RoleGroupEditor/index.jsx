import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import util from "../../../../utils";
import resx from "../../../../resources";
import Validator from "../../../common/validator";
import SingleInput from "dnn-single-line-input";
import MultiLineInput from "dnn-multi-line-input";
import Button from "dnn-button";
import IconButton from "../../../common/IconButton";
import ReactDOM from "react-dom";
import Label from "dnn-label";
import {
    roles as RolesActions
} from "../../../../actions";

class RoleGroupEditor extends Component {
    constructor(props) {
        super(props);
        let group = Object.assign({}, props.group);

        this.state = {
            showPopup: props.showPopup,
            showActions: props.showActions,
            group
        };
        this.handleClick = this.handleClick.bind(this);
    }
    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    componentWillReceiveProps(newProps) {
        this.setState(newProps);
    }
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if (ReactDOM.findDOMNode(this) !== null && !ReactDOM.findDOMNode(this).contains(event.target) &&
            (event.target.firstChild !== null && typeof event.target.firstChild.className === "string" && event.target.firstChild.className.indexOf("do-not-close") === -1)) {
            if (typeof this.props.onCancel === "function") {
                this.props.onCancel();
            }
            this.closeCreateGroup();
        }
    }
    onEditFieldChanged(name, event) {
        let {group} = this.state;
        group[name] = event.target.value;
        this.setState({
            group
        });
    }

    onCancel() {
        if (typeof this.props.onCancel === "function") {
            this.props.onCancel();
        }
        this.closeCreateGroup();
    }

    onSave() {
        const {props, state} = this;

        if (!Validator.validate("rolegroup")) {
            return;
        }
        props.dispatch(RolesActions.saveRoleGroup(state.group, (group) => {
            util.utilities.notify(resx.get("RoleGroupUpdated.Message"));
            if (typeof this.props.onSave === "function") {
                this.props.onSave(group);
            }
            this.closeCreateGroup();
        }, () => {
            util.utilities.notify(resx.get("RoleGroupUpdated.Error"));
        }));
    }
    onEdit() {
        if (typeof this.props.onEdit === "function") {
            this.props.onEdit();
        }
        let {showPopup} = this.state;
        if (!showPopup) {
            showPopup = true;
            this.setState({
                showPopup
            });
        }
    }
    onDelete() {
        const {props, state} = this;
        props.onDeleteClick();
        util.utilities.confirm(resx.get("DeleteRoleGroup.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RolesActions.deleteRoleGroup(state.group, (group) => {
                util.utilities.notify(resx.get("DeleteRoleGroup.Message"));
                if (typeof props.onDeleted === "function") {
                    props.onDeleted(group);
                }
            }, (error) => {
                util.utilities.notify(resx.get("DeleteRoleGroup.Error"));
            }));
        }, () => {
            util.utilities.notify(resx.get("ConfigDeleteCancelled"));
        });
        this.closeCreateGroup();
    }
    closeCreateGroup() {
        let {showPopup} = this.state;
        showPopup = false;
        this.setState({
            showPopup,
            group: {}
        });
    }

    render() {
        const {props, state} = this;
        let group = Object.assign({}, state.group);
        return props.visible && <div className="role-group-editor" onClick={this.props.onClick.bind(this) }>
            {
                state.showActions &&
                <div className="role-group-actions">
                    <IconButton type="Edit" onClick={this.onEdit.bind(this) } />
                    {this.props.deleteAllowed && <IconButton type="Trash" onClick={this.onDelete.bind(this) } />}
                </div>
            }
            {
                state.showPopup &&
                <div className="popup popup-editgroup">
                    <h2>{resx.get(props.title) }</h2>
                    <div className="edit-form">
                        <div className="form-items">
                            <div className="form-item">
                                <Validator required={true} errorMessage={resx.get("GroupName.Required") } group="rolegroup" name="groupname">
                                    <Label label={resx.get("GroupName") } tooltipMessage={resx.get("GroupName.Help") } tooltipPlace={"top"} />
                                    <SingleInput type="text" value={group.name} onChange={this.onEditFieldChanged.bind(this, "name") } maxLength={50} />
                                </Validator>
                            </div>
                            <div className="form-item">
                                <Label label={resx.get("GroupDescription") } tooltipMessage={resx.get("GroupDescription.Help") } tooltipPlace={"top"} />
                                <MultiLineInput value={group.description} onChange={this.onEditFieldChanged.bind(this, "description") } maxLength={500} />
                            </div>
                            <div className="clear"></div>
                        </div>
                        <div className="actions">
                            <Button onClick={this.onCancel.bind(this) } className="do-not-close">{resx.get("Cancel") }</Button>
                            <Button type="primary" onClick={this.onSave.bind(this) }>{resx.get("Save") }</Button>
                        </div>
                    </div>
                </div>
            }
        </div >;
    }
}

RoleGroupEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    title: PropTypes.string,
    visible: PropTypes.bool,
    group: PropTypes.object,
    deleteAllowed: PropTypes.bool,
    showActions: PropTypes.bool,
    showPopup: PropTypes.bool,
    onDeleteClick: PropTypes.func,
    onDeleted: PropTypes.func,
    onEdit: PropTypes.func,
    onSave: PropTypes.func,
    onClick: PropTypes.func,
    onCancel: PropTypes.func
};
RoleGroupEditor.defaultProps = {
    showPopup: false,
    showActions: false,
    deleteAllowed: false,
    visible: true
};

function mapStateToProps(state) {
    return {
        //roleGroups: state.roles.roleGroups
    };
}

export default connect(mapStateToProps)(RoleGroupEditor);