import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import ReactDOM from "react-dom";
import Menu from "./menu/Menu";
import MenuItem from "./menu/MenuItem";
import Localization from "localization";
import {users as UserActions} from "../../../../actions";
import util from "../../../../utils";
import GridCell from "dnn-grid-cell";
import ChangePassword from "../ChangePassword";
import "./style.less";

class UserMenu extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userDetails: props.userDetails,
            ChangePasswordVisible: false
        };
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(e) {
        if (!ReactDOM.findDOMNode(this).contains(e.target) && (typeof event.target.className === "string" && event.target.className.indexOf("menu-item") === -1)) {
            this.props.onClose();
        }
    }
    componentWillMount() {
        document.addEventListener("click", this.handleClick, false);
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props);
        }
    }
    componentWillReceiveProps(newProps) {
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId) {
            this.getUserDetails(newProps);
        }
    }
    getUserDetails(props) {
        props.dispatch(UserActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            });
        }));
    }
    reload() {
        this.getUserDetails(this.props);
    }
    componentWillUnmount() {
        //this.props.onClose();
        document.removeEventListener("click", this.handleClick, false);
    }
    sort(items, column, order) {
        order = order === undefined ? "asc" : order;
        items = items.sort(function (a, b) {
            if (a[column] > b[column]) //sort string descending
                return order === "asc" ? 1 : -1;
            if (a[column] < b[column])
                return order === "asc" ? -1 : 1;
            return 0;//default return value (no sorting)
        });
        return items;
    }
    onItemClick(key) {
        switch (key) {
            case "SendPasswordResetLink":
                this.onSendPasswordLink();
                this.props.onClose();
                break;
            case "ChangePassword":
                this.toggleChangePassword();
                break;
            case "DeleteUser":
                this.deleteUser();
                this.props.onClose();
                break;
            case "DeAuthorizeUser":
                this.updateAuthorizeStatus(false);
                this.props.onClose();
                break;
            case "AuthorizeUser":
                this.updateAuthorizeStatus(true);
                this.props.onClose();
                break;
            case "MakeSuperUser":
                this.makeSuperUser();
                this.props.onClose();
                break;
        }
    }
    onSendPasswordLink() {
        this.props.dispatch(UserActions.sendPasswordResetLink({ userId: this.props.userId }, (data) => {
            if (data.Success)
                util.utilities.notify("Password reset link sent successfully");
            else {
                util.utilities.notify(data.Message);
            }
        }));
    }
    deleteUser() {
        util.utilities.confirm(Localization.get("DeleteUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(UserActions.deleteUser({ userId: this.props.userId }, (data) => {
                if (data.Success)
                    util.utilities.notify("User deleted successfully");
                else {
                    util.utilities.notify(data.Message);
                }
            }));
        });
    }
    updateAuthorizeStatus(authorized) {
        this.props.dispatch(UserActions.updateAuthorizeStatus({ userId: this.props.userId, authorized: authorized }, (data) => {
            if (data.Success) {
                util.utilities.notify(authorized ? "User authorized successfully" : "User un-authorized successfully");
                this.reload();
            }
            else {
                util.utilities.notify(data.Message);
            }
        }));
    }
    makeSuperUser() {
        this.props.dispatch(UserActions.updateSuperUserStatus({ userId: this.props.userId, setSuperUser: true }, (data) => {
            if (data.Success)
                util.utilities.notify("User made super user");
            else {
                util.utilities.notify(data.Message);
            }
        }));
    }

    toggleChangePassword() {
        const show = !this.state.ChangePasswordVisible;
        this.setState({ ChangePasswordVisible: show });
    }
    render() {
        let visibleMenus = [{ title: "ViewProfile", index: 0 },
            { title: "ViewAssets", index: 1 },
            { title: "ChangePassword", index: 2 },
            { title: "SendPasswordResetLink", index: 3 }

        ];

        //if (1 === 1) {
        visibleMenus = [{ title: "MakeSuperUser", index: 8 }].concat(visibleMenus);
        //}
        if (this.state.userDetails.needUpdatePassword) {
            visibleMenus = [{ title: "ForceChangePassword", index: 4 }].concat(visibleMenus);
        }
        if (this.state.userDetails.isDeleted) {
            visibleMenus = [{ title: "RestoreUser", index: 7 }].concat(visibleMenus);
            visibleMenus = [{ title: "EraseUser", index: 6 }].concat(visibleMenus);
        } else {
            visibleMenus = [{ title: "DeleteUser", index: 6 }].concat(visibleMenus);
        }
        if (this.state.userDetails.authorized) {
            visibleMenus = [{ title: "DeAuthorizeUser", index: 5 }].concat(visibleMenus);
        } else {
            visibleMenus = [{ title: "AuthorizeUser", index: 5 }].concat(visibleMenus);
        }
        visibleMenus = this.sort(visibleMenus, "index");

        return (
            <GridCell className="dnn-user-menu menu-popup">
                {!this.state.ChangePasswordVisible &&
                    <Menu>
                        {
                            visibleMenus.map(menu => {
                                return <MenuItem onMenuAction={this.onItemClick.bind(this, menu.title) }>{Localization.get(menu.title) }</MenuItem>;
                            })
                        }
                    </Menu>
                }
                {this.state.ChangePasswordVisible &&
                    <ChangePassword onCancel={this.toggleChangePassword.bind(this) } userId={this.props.userId} />
                }
            </GridCell>
        );
    }
}

UserMenu.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.number.isRequired,
    onClose: PropTypes.func.isRequired,
    userDetails: PropTypes.object
};

function mapStateToProps(state) {
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserMenu);