import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import HeaderRow from "./HeaderRow";
import DetailRow from "./DetailRow";
import CollapsibleSwitcher from "../common/CollapsibleSwitcher";
import CreateUserBox from "../CreateUserBox";
import UserSettings from "./UserSettings";
import EditProfile from "./EditProfile";
import UsersRoles from "./UsersRoles";
import styles from "./style.less";
import {sort} from "../../helpers";
import Localization from "localization";
import ColumnSizes from "./columnSizes";
import {canManageRoles, canManageProfile, canViewSettings, canAddUser} from "../permissionHelpers.js";
import { GridCell } from "@dnnsoftware/dnn-react-common";

class UserTable extends Component {
    constructor() {
        super();
        this.state = {
            openId: "",
            renderIndex: -1
        };
    }

    componentDidMount() {
        this.collapse();
    }

    unCollapse(id, index) {
        this.setState({
            openId: id,
            renderIndex: index
        });
    }
    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: "",
                renderIndex: -1
            });
        }
    }
    toggle(openId, index) {
        if (openId !== "") {
            this.unCollapse(openId, index);
        } else {
            this.collapse();
        }
    }
    onAddUser() {
        this.toggle(this.state.openId === "add" ? "" : "add", 0);
    }
    getChildren(user) {
        let children = [];
        children = children.concat((this.props.getUserTabs && this.props.getUserTabs(user)) || []);
        if (canViewSettings(this.props.appSettings.applicationSettings.settings))
        {
            const userSettings = <UserSettings key={`usersettings-${user.userId}`} userId={user.userId} collapse={this.collapse.bind(this) } appSettings={this.props.appSettings}/>;
            children = children.concat([{
                index: 10,
                content: userSettings
            }]);
        }

        if (canManageRoles(this.props.appSettings.applicationSettings.settings, user))
        {
            children = children.concat([{
                index: 5,
                content: <UsersRoles key={`usersroles-${user.userId}`} userDetails={user} />
            }]);
        }

        if (canManageProfile(this.props.appSettings.applicationSettings.settings))
        {
            children = children.concat([{
                index: 15,
                content: <EditProfile key={`editprofile-${user.userId}`} userId={user.userId} />
            }]);
        }
        return sort(children, "index", "desc").map((child) => {
            return child.content;
        });
    }
    getHeaders()
    {
        let columnSizes =this.props.columnSizes!==undefined? this.props.columnSizes: ColumnSizes;
        let headers = [{index: 5, size: columnSizes.find(x=>x.index===5).size, header: Localization.get("Name.Header")},
            {index: 10, size: columnSizes.find(x=>x.index===10).size, header: Localization.get("Email.Header")},
            {index: 15, size: columnSizes.find(x=>x.index===15).size, header: Localization.get("Created.Header")},
            {index: 25, size: columnSizes.find(x=>x.index===25).size, header:""}];
        if (this.props.getUserColumns !== undefined  && typeof this.props.getUserColumns ==="function") {
            let extraColumns = this.props.getUserColumns();
            if (extraColumns!==undefined && extraColumns.length>0)
            {
                headers = sort(extraColumns.map(column=>{
                    return {
                        index:column.index,
                        header:column.header,
                        size: columnSizes.find(x=>x.index===column.index).size
                    };
                }).concat(headers), "index");
            }
        }
        return headers;
    }
    
    render() {
        const {props} = this;
        let i = 0;
        let opened = (this.state.openId === "add");
        const addIsOpened = opened && canAddUser(this.props.appSettings.applicationSettings.settings);
        const headers = this.getHeaders();
        const createUserBox = () => [<CreateUserBox key={`create-user-box-${i++}`} filter={props.filter} onCancel={this.collapse.bind(this) } appSettings={props.appSettings}/>];
        return (
            <GridCell className={styles.usersList}>
                <HeaderRow headers={headers}/>
                <DetailRow
                    Collapse={this.toggle.bind(this) }
                    OpenCollapse={this.toggle.bind(this) }
                    currentIndex={this.state.renderIndex}
                    openId={this.state.openId }
                    key={"user-add"}
                    appSettings={props.appSettings}
                    columnSizes={props.columnSizes}
                    id={"add"}
                    addIsOpened={addIsOpened ? "add-opened" : "closed"}
                    getUserMenu={props.getUserMenu && props.getUserMenu.bind(this)}
                    userMenuAction={props.userMenuAction && props.userMenuAction.bind(this)}
                    filter={props.filter}>
                    <CollapsibleSwitcher>
                        {createUserBox()}
                    </CollapsibleSwitcher>
                </DetailRow>
                {
                    props.users && props.users.length>0 && props.users.map((user, index) => {
                        let id = "row-" + i++;
                        const children = this.getChildren(user);
                        return <DetailRow
                            user={user}
                            Collapse={this.collapse.bind(this) }
                            OpenCollapse={this.toggle.bind(this) }
                            currentIndex={this.state.renderIndex}
                            openId={this.state.openId }
                            key={"user-" + index}
                            getUserColumns={props.getUserColumns && props.getUserColumns.bind(this) }
                            getUserTabsIcons={props.getUserTabsIcons && props.getUserTabsIcons.bind(this) }
                            getUserMenu={props.getUserMenu && props.getUserMenu.bind(this)} 
                            userMenuAction={props.userMenuAction && props.userMenuAction.bind(this)}
                            appSettings={props.appSettings}
                            columnSizes={props.columnSizes}
                            id={id}
                            filter={props.filter}>
                            <CollapsibleSwitcher renderIndex={this.state.renderIndex}>
                                {children}
                            </CollapsibleSwitcher>
                        </DetailRow>;
                    }) 
                }
                {
                    props.users && props.users.length === 0 && <GridCell className="no-users">{Localization.get("noUsers")}</GridCell>
                }
            </GridCell>
        );
    }
}

UserTable.propTypes = {
    dispatch: PropTypes.func.isRequired,
    getUserTabs: PropTypes.func,
    getUserTabsIcons: PropTypes.func,
    getUserColumns: PropTypes.func,
    getUserMenu: PropTypes.func,
    userMenuAction: PropTypes.func,
    appSettings: PropTypes.object,
    columnSizes: PropTypes.array,
    filter: PropTypes.number
};
function mapStateToProps(state) {
    return {
        users: state.users.users
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(UserTable);