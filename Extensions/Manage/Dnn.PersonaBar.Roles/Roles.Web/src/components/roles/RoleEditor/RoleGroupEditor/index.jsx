import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import util from "../../../../utils";
import resx from "../../../../resources";
import { SingleLineInputWithError, MultiLineInput, Button, Label }  from "@dnnsoftware/dnn-react-common";
import {
    roles as RolesActions
} from "../../../../actions";

class RoleGroupEditor extends Component {
    constructor(props) {
        super(props);
        let group = Object.assign({}, props.group);

        this.state = {
            group,
            errors: {
                groupName: false
            }
        };
        this.submitted = false;
        this.handleClick = this.handleClick.bind(this);
        this.node = React.createRef();
    }
    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }

    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        var node = this.node.current;
        if (node && !node.contains(event.target) &&
            (event.target.firstChild !== null && typeof event.target.firstChild.className === "string" && event.target.firstChild.className.indexOf("do-not-close") === -1)) {
            if (typeof this.props.onCancel === "function") {
                this.props.onCancel();
            }
            this.props.onCancel();
        }
    }
    onEditFieldChanged(name, event) {
        let {group} = this.state;
        group[name] = event.target.value;
        this.setState({
            group
        }, () => {
            this.validateForm();
        });
    }

    onCancel() {
        this.setState({
            group: {}
        }, () => {
            if (typeof this.props.onCancel === "function") {
                this.props.onCancel();
            }
        });
    }

    onSave() {
        const {props, state} = this;
        this.submitted = true;
        if (this.validateForm()) {
            props.dispatch(RolesActions.saveRoleGroup(state.group, (group) => {
                util.utilities.notify(resx.get("RoleGroupUpdated.Message"));
                if (typeof this.props.onSave === "function") {
                    this.props.onSave(group);
                }
            }));
        }
    }


    validateForm() {
        let valid = true;
        if (this.submitted) {
            let {group} = this.state;
            let {errors} = this.state;
            errors.groupName = false;
            if (group.name === "") {
                errors.groupName = true;
                valid = false;
            }
            this.setState({ errors });
        }
        return valid;
    }


    render() {
        const {props, state} = this;
        let group = Object.assign({}, state.group);
        return props.visible && <div ref={node => this.node = node} className="role-group-editor" onClick={this.props.onClick.bind(this) }>
            <h2>{resx.get(props.title) }</h2>
            <div className="edit-form">
                <div className="form-items">
                    <div className="form-item">
                        <SingleLineInputWithError  value={group.name} onChange={this.onEditFieldChanged.bind(this, "name") } maxLength={50}
                            error={state.errors.groupName} label={resx.get("GroupName") }
                            tooltipMessage={resx.get("GroupName.Help") } errorMessage={resx.get("GroupName.Help") }
                            autoComplete="off"
                            inputStyle={{ marginBottom: 15 }}
                            tabIndex={1}/>
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
        </div>;
    }
}

RoleGroupEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    title: PropTypes.string,
    visible: PropTypes.bool,
    group: PropTypes.object,
    deleteAllowed: PropTypes.bool,
    onDeleteClick: PropTypes.func,
    onDeleted: PropTypes.func,
    onSave: PropTypes.func,
    onClick: PropTypes.func,
    onCancel: PropTypes.func
};
RoleGroupEditor.defaultProps = {
    deleteAllowed: false,
    visible: true,
    group: {
        id: -2,
        name: "",
        description: ""
    }
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(RoleGroupEditor);