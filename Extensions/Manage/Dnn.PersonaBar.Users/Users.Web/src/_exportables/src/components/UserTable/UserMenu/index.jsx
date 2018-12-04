import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import Menu from "./menu/Menu";
import MenuItem from "./menu/MenuItem";
import Localization from "localization";
import { CommonUsersActions } from "../../../actions";
import utilities from "utils";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import ChangePassword from "../ChangePassword";
import "./style.less";
import {canManagePassword, canDeleteUser, canAuthorizeUnAuthorizeUser, canPromoteDemote} from "../../permissionHelpers.js";

class UserMenu extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userDetails: props.userDetails,
            ChangePasswordVisible: false
        };
        this.showMenu = false;
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(event) {
        if ((typeof event.target.className !== "string" || (typeof event.target.className === "string" && event.target.className.indexOf("menu-item") === -1))) {
            this.props.onClose();
        }
    }
    componentWillMount() {
        document.addEventListener("click", this.handleClick, false);
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.showMenu = false;
            this.getUserDetails(props);
        }
        else {
            this.showMenu = true;
        }
    }
    componentWillReceiveProps(newProps) {
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId) {
            this.showMenu = false;
            this.getUserDetails(newProps);
        }
        else {
            this.showMenu = true;
        }
    }
    getUserDetails(props) {
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            },()=>{
                this.showMenu = true;
            });
        }));
    }
    reload() {
        this.getUserDetails(this.props);
    }
    componentWillUnmount() {
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
            case "ResetPassword":
                this.onSendPasswordLink();
                this.props.onClose();
                break;
            case "ChangePassword":
                this.toggleChangePassword();
                break;
            case "ForceChangePassword":
                this.forcePasswordChange();
                this.props.onClose();
                break;
            case "DeleteUser":
                this.deleteUser();
                this.props.onClose();
                break;
            case "RemoveUser":
                this.hardDeleteUser();
                this.props.onClose();
                break;
            case "RestoreUser":
                this.restoreUser();
                this.props.onClose();
                break;
            case "cmdUnAuthorize":
                this.updateAuthorizeStatus(false);
                this.props.onClose();
                break;
            case "cmdAuthorize":
                this.updateAuthorizeStatus(true);
                this.props.onClose();
                break;
            case "cmdUnLock":
                this.unLockUser();
                this.props.onClose();
                break;
            case "PromoteToSuperUser":
                this.updateSuperUserStatus(true);
                this.props.onClose();
                break;
            case "DemoteToRegularUser":
                this.updateSuperUserStatus(false);
                this.props.onClose();
                break;
            case "ViewProfile":
                this.onViewProfile();
                this.props.onClose();
                break;
            default:
                if (typeof this.props.userMenuAction === "function")
                    this.props.userMenuAction(key, this.state.userDetails);
                this.props.onClose();
                break;
        }
    }
    
    onViewProfile() {
        utilities.closePersonaBar(() => {
            window.top.location = this.state.userDetails.profileUrl;
        });
    }
    onSendPasswordLink() {
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("PasswordSent"), 10000);
        }));
    }
    deleteUser() {
        utilities.confirm(Localization.get("DeleteUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.deleteUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
                utilities.notify(Localization.get("UserDeleted"), 3000);
                this.reload();
            }));
        });
    }
    hardDeleteUser() {
        utilities.confirm(Localization.get("RemoveUser.Confirm"), Localization.get("Delete"), Localization.get("Cancel"), () => {
            this.props.dispatch(CommonUsersActions.eraseUser({ userId: this.props.userId }));
        });
    }
    restoreUser() {
        this.props.dispatch(CommonUsersActions.restoreUser({ userDetails: this.props.userDetails }, this.props.filter, () => {
            utilities.notify(Localization.get("UserRestored"), 3000);
            this.reload();
        }));
    }
    forcePasswordChange() {
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("UserPasswordUpdateChanged"), 10000);
            this.reload();
        }));
    }
    updateAuthorizeStatus(authorized) {
        this.props.dispatch(CommonUsersActions.updateAuthorizeStatus({ userDetails: this.props.userDetails }, authorized, this.props.filter, () => {
            utilities.notify(authorized ? Localization.get("UserAuthorized") :Localization.get("UserUnAuthorized"), 3000);
            this.reload();
        }));
    }
    unLockUser() {
        this.props.dispatch(CommonUsersActions.unLockUser({ userDetails: this.props.userDetails }, () => {
            utilities.notify(Localization.get("UserUnLocked"), 3000);
            this.reload();
        }));
    }
    updateSuperUserStatus(setSuperUser) {
        this.props.dispatch(CommonUsersActions.updateSuperUserStatus({ userId: this.props.userId, setSuperUser: setSuperUser }, this.props.filter, () => {
            this.reload();
        }));
    }
    toggleChangePassword(close) {
        const show = !this.state.ChangePasswordVisible;
        this.setState({ ChangePasswordVisible: show });
        if (close)
            this.props.onClose();
    }
    
    render() {

        let visibleMenus = [{ key:"ViewProfile", title:  Localization.get("ViewProfile"), index: 10 }];
        
        if (canPromoteDemote(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (!this.state.userDetails.isSuperUser) {
                visibleMenus = [{ key:"PromoteToSuperUser", title:  Localization.get("PromoteToSuperUser"), index: 80 }].concat(visibleMenus);
            }
            else if (this.state.userDetails.isSuperUser)
            {
                visibleMenus = [{ key:"DemoteToRegularUser", title:  Localization.get("DemoteToRegularUser"), index: 80 }].concat(visibleMenus);
            }
        }
        if (canManagePassword(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            visibleMenus = [{ key:"ResetPassword", title: Localization.get("ResetPassword"), index: 40 }].concat(visibleMenus);
            visibleMenus = [{ key:"ChangePassword", title: Localization.get("ChangePassword"), index: 30 }].concat(visibleMenus);
            if (!this.state.userDetails.needUpdatePassword) {
                visibleMenus = [{ key:"ForceChangePassword", title:  Localization.get("ForceChangePassword"), index: 40 }].concat(visibleMenus);
            }
        }
        if (canDeleteUser(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (this.state.userDetails.isDeleted) {
                visibleMenus = [{ key:"RestoreUser", title:  Localization.get("RestoreUser"), index: 70 }].concat(visibleMenus);
                visibleMenus = [{ key:"RemoveUser", title:  Localization.get("RemoveUser"), index: 60 }].concat(visibleMenus);
            } else
            {
                visibleMenus = [{ key:"DeleteUser", title:  Localization.get("DeleteUser"), index: 60 }].concat(visibleMenus);
            }
        }
        if (canAuthorizeUnAuthorizeUser(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId))
        {
            if (this.state.userDetails.authorized) {
                visibleMenus = [{ key:"cmdUnAuthorize", title:  Localization.get("cmdUnAuthorize"), index: 50 }].concat(visibleMenus);
            } 
            else
            {
                visibleMenus = [{ key:"cmdAuthorize", title:  Localization.get("cmdAuthorize"), index: 50 }].concat(visibleMenus);
            }
            if (this.state.userDetails.isLocked) {
                visibleMenus = [{ key:"cmdUnLock", title:  Localization.get("cmUnlockUser"), index: 100 }].concat(visibleMenus);
            }
        }
       
        visibleMenus = visibleMenus.concat((this.props.getUserMenu && this.props.getUserMenu(this.state.userDetails)) || []);

        visibleMenus = this.sort(visibleMenus, "index");
        let showMenu = this.showMenu;
        
        if (showMenu)
        {
            return ( <GridCell className="dnn-user-menu menu-popup" ref={(node) => this.rootElement = node}>
                {!this.state.ChangePasswordVisible &&
                    <Menu>
                        {
                            visibleMenus.map((menu, index) => {
                                return <MenuItem key={`menu_item_${index}`} onMenuAction={this.onItemClick.bind(this, menu.key) }>{menu.title}</MenuItem>;
                            })
                        }
                    </Menu>
                }
                {this.state.ChangePasswordVisible &&
                    <ChangePassword onCancel={this.toggleChangePassword.bind(this, true) } userId={this.props.userId} />
                }
            </GridCell>
            );
        }
        else
        {
            return <div/>;
        }
    }
}

UserMenu.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.number.isRequired,
    onClose: PropTypes.func.isRequired,
    userDetails: PropTypes.object,
    getUserMenu: PropTypes.func.isRequired,
    userMenuAction: PropTypes.func.isRequired,
    appSettings: PropTypes.object,
    filter: PropTypes.number
};

function mapStateToProps(state) {
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserMenu);