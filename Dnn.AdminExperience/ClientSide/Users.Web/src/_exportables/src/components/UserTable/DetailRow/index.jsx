import PropTypes from "prop-types";
import React, { Component } from "react";
import styles from "./style.less";
import {formatDate, sort} from "../../../helpers";
import UserMenu from "../UserMenu";
import Localization from "localization";
import ColumnSizes from "../columnSizes";
import {canManageRoles, canManageProfile, canViewSettings} from "../../permissionHelpers.js";
import { SvgIcons, GridCell, Collapsible, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

class DetailsRow extends Component {
    constructor() {
        super();
        this.rootElement = React.createRef();
        this.state = {
            opened: false,
            showMenu: false
        };
    }
    componentDidMount() {
        this._isMounted = true;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }

    componentWillUnmount() {
        this._isMounted = false;
    }

    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if ((typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)
            && !(event.target.id === "confirmbtn" || event.target.id === "cancelbtn") && this.props.openId !== "add") {
            if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
                this.props.Collapse();
            }
        }
    }
    toggle(index) {
        if ((this.props.openId !== "" && this.props.id === this.props.openId) && this.props.currentIndex === index) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id, index);
        }
    }
    toggleUserMenu() {
        const show = !this.state.showMenu;
        this.setState({ showMenu: show });
    }
    
    /* eslint-disable react/no-danger */
    getUserActions(user, opened) {
        let actionIcons = [];
        actionIcons = actionIcons.concat((this.props.getUserTabsIcons && this.props.getUserTabsIcons(user)) || []);

        if (canManageProfile(this.props.appSettings.applicationSettings.settings))
        {
            actionIcons = actionIcons.concat([{
                index: 15,
                icon: SvgIcons.UserIcon,
                title: Localization.get("ManageProfile.title")
            }]);
        }
        if (canViewSettings(this.props.appSettings.applicationSettings.settings)) {
            actionIcons = actionIcons.concat([{
                index: 10,
                icon: SvgIcons.SettingsIcon,
                title: Localization.get("ManageSettings.title")
            }]);
        }

        if (canManageRoles(this.props.appSettings.applicationSettings.settings, user))
        {
            actionIcons = actionIcons.concat([{
                index: 5,
                icon: SvgIcons.ShieldIcon,
                title: Localization.get("ManageRoles.title")
            }]);
        }

        let i = 0;
        let userActions = sort(actionIcons, "index", "desc").map((actionIcon) => {
            let element = <div key={`user_action_${i}`} title={actionIcon.title} className={ "extension-action " + !(opened && this.props.currentIndex === i) } dangerouslySetInnerHTML={{ __html: actionIcon.icon }} onClick={ this.toggle.bind(this, i) } ></div>;
            i++;
            return element;
        });
        return ([<div key={`user_action_wrapper_${user.userId}`} style={{ position: "relative" }}>
            <div className={"extension-action " + !this.state.showMenu} dangerouslySetInnerHTML={{ __html: SvgIcons.MoreMenuIcon }}
                onClick={this.toggleUserMenu.bind(this) }>
            </div>
            { this.state.showMenu && <UserMenu filter={this.props.filter} appSettings={this.props.appSettings} getUserMenu={this.props.getUserMenu && this.props.getUserMenu.bind(this)} userMenuAction={this.props.userMenuAction && this.props.userMenuAction.bind(this)} onClose={this.toggleUserMenu.bind(this) } 
                userId={user.userId}/> }
        </div>]).concat(userActions);
    }
    getUserColumns(user, id, opened) {
        let userActions = this.getUserActions(user, opened);
        let extraColumns = this.props.getUserColumns && this.props.getUserColumns(user);
        let columnSizes =this.props.columnSizes!==undefined? this.props.columnSizes: ColumnSizes;
        let userColumns = [];
        if (this.props.appSettings.applicationSettings.settings.dataConsentActive) {
            let statusClass = "black";
            let statusIcon = SvgIcons.Signature;
            let hoverText = Localization.get("HasAgreedToTerms.title");
            if (user.requestsRemoval) {
                statusClass = "red";
                statusIcon = SvgIcons.UserSlash;
                hoverText = Localization.get("RequestsRemoval.title");
            } else if (user.isDeleted) {
                statusClass = "grey";
                statusIcon = SvgIcons.UserSlash;
                hoverText = Localization.get("Deleted");
            } else if (!user.authorized) {
                statusClass = "grey";
                statusIcon = SvgIcons.ShieldIcon;
                hoverText = Localization.get("UnAuthorized");
            } else if (!user.hasAgreedToTerms) {
                statusClass = "grey";
                hoverText = Localization.get("HasNotAgreedToTerms.title");
            } 
            userColumns = [
                {
                    index: 3,
                    content: <GridCell key={`gc-userstatus-${user.userId}`} columnSize={columnSizes.find(x=>x.index===3).size}>
                            <span dangerouslySetInnerHTML={{__html: statusIcon}} className={"user-status " + statusClass} title={hoverText}></span>
                        </GridCell>
                }]
        }
        userColumns = userColumns.concat([
            {
                index: 5,
                content: <GridCell key={`gc-username-${user.userId}`} columnSize={columnSizes.find(x=>x.index===5).size}  className={"user-names" + (user.isDeleted ? " deleted" : "") }>
                    <h6>
                        <TextOverflowWrapper className="email-link" text={user.displayName} maxWidth={125}/>
                    </h6>
                    {user.displayName !== "-" && <p>{user.userName}</p> }
                </GridCell>
            },
            {
                index: 10,
                content: <GridCell key={`gc-email-link-${user.userId}`} columnSize={columnSizes.find(x=>x.index===10).size}  className={"user-emails" + (user.isDeleted ? " deleted" : "") } >
                    <TextOverflowWrapper className="email-link" isAnchor={true} href={"mailto:" + user.email} text={user.email} maxWidth={125}/>
                </GridCell >
            },
            {
                index: 15,
                content: <GridCell key={`gc-createdon-${user.userId}`} columnSize={columnSizes.find(x=>x.index===15).size}  className={"user-joined" + (user.isDeleted ? " deleted" : "")}>
                    {user.createdOnDate !== "-" && <p>{formatDate(user.createdOnDate) }</p>}
                    {user.createdOnDate === "-" && user.createdOnDate}
                </GridCell>
            },
            {
                index: 25,
                content: id !== "add" && <GridCell key={`gc-actions-${user.userId}`} columnSize={columnSizes.find(x=>x.index===25).size} style={{float:"right", textAlign:"right", paddingRight: 2}}>{userActions}</GridCell>
            }
        ]).concat((extraColumns) || []);

        return sort(userColumns, "index").map((column) => {
            return column.content;
        });
    }

    render() {
        const {props} = this;
        let {user} = this.props;
        let opened = (props.openId !== "" && props.id === props.openId);
        let uniqueId = "userRow-" + Math.random() + Date.now();
        if (user === undefined) {
            user = {
                avatar: "-",
                displayName: "-",
                userName: "-",
                email: "-",
                createdOnDate: "-",
                authorized: "-"
            };
        }
        let userColumns = this.getUserColumns(user, props.id, opened);
        return (
            /* eslint-disable react/no-danger */
            <GridCell className={"collapsible-component-users"} id={uniqueId} ref={(node) => this.rootElement = node}>
                <GridCell  className={"collapsible-header-users " + !opened}>
                    <GridCell className={styles.extensionDetailRow + " " + props.addIsOpened} columnSize={100}>
                        {(!props.addIsOpened || props.addIsOpened === "add-opened") && <GridCell>
                            {userColumns}
                        </GridCell>}
                        <Collapsible accordion={true} isOpened={opened} keepCollapsedContent={true} className="user-detail-row">
                            {opened && props.children }
                        </Collapsible>
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}

DetailsRow.propTypes = {
    user: PropTypes.object,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    currentIndex: PropTypes.number,
    getUserTabsIcons: PropTypes.func,
    getUserColumns: PropTypes.func,
    getUserMenu: PropTypes.func,
    userMenuAction: PropTypes.func,
    appSettings: PropTypes.object,
    columnSizes: PropTypes.array,
    filter: PropTypes.number
};

DetailsRow.defaultProps = {
    isEvoq: false
};

export default DetailsRow;