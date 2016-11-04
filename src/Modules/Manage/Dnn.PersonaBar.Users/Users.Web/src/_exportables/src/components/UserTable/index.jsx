import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import HeaderRow from "./HeaderRow";
import DetailRow from "./DetailRow";
import GridCell from "dnn-grid-cell";
import CollapsibleSwitcher from "../common/CollapsibleSwitcher";
import CreateUserBox from "../CreateUserBox";
import UserSettings from "./UserSettings";
import EditProfile from "./EditProfile";
import UsersRoles from "./UsersRoles";
import styles from "./style.less";
import {sort} from "../../helpers";
import Localization from "localization";

class UserTable extends Component {
    constructor() {
        super();
        this.state = {
            openId: "",
            renderIndex: -1
        };
    }
    uncollapse(id, index) {
        setTimeout(() => {
            this.setState({
                openId: id,
                renderIndex: index
            });
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
            this.uncollapse(openId, index);
        } else {
            this.collapse();
        }
    }
    onAddUser() {
        this.toggle(this.state.openId === "add" ? "" : "add", 0);
    }
    getChildren(user) {
        let children = [
            {
                index: 5,
                content: <UsersRoles userDetails={user} />
            },
            {
                index: 10,
                content: <EditProfile  userId={user.userId} />
            },
            {
                index: 15,
                content: <UserSettings userId={user.userId} collapse={this.collapse.bind(this) }/>
            }
        ].concat((this.props.getUserTabs && this.props.getUserTabs(user)) || []);
        return sort(children, "index").map((child) => {
            return child.content;
        });
    }
    getHeaders()
    {
        let headers = [{index: 5, header: Localization.get("Name.Header")},
                    {index: 10,header: Localization.get("Email.Header")},
                    {index: 15,header: Localization.get("Created.Header")},
                    //{index: 20,header: Localization.get("Authorized.Header")},
                    {index: 25,header:""}];
        if (this.props.getUserColumns !== undefined  && typeof this.props.getUserColumns ==="function"){
            let extraColumns = this.props.getUserColumns();
            if (extraColumns!==undefined && extraColumns.length>0)
            {
                headers = sort(extraColumns.map(column=>{
                    return {
                        index:column.index,
                        header:column.header
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
        const headers = this.getHeaders();
        return (
            <GridCell className={styles.usersList}>
                <HeaderRow headers={headers}/>
                {opened && <DetailRow
                    Collapse={this.collapse.bind(this) }
                    OpenCollapse={this.toggle.bind(this) }
                    currentIndex={this.state.renderIndex}
                    openId={this.state.openId }
                    key={"user-add"}
                    id={"add"}>
                    <CollapsibleSwitcher children={[<CreateUserBox onCancel={this.collapse.bind(this) }/>]}/>
                </DetailRow>
                }
                {
                    props.users && props.users.length>0 && props.users.map((user, index) => {
                        let id = "row-" + i++;
                        return <DetailRow
                            user={user}
                            Collapse={this.collapse.bind(this) }
                            OpenCollapse={this.toggle.bind(this) }
                            currentIndex={this.state.renderIndex}
                            openId={this.state.openId }
                            key={"user-" + index}
                            getUserColumns={this.props.getUserColumns && this.props.getUserColumns.bind(this) }
                            getUserTabsIcons={this.props.getUserTabsIcons && this.props.getUserTabsIcons.bind(this) }
                            getUserMenu={this.props.getUserMenu && this.props.getUserMenu.bind(this)} 
                            userMenuAction={this.props.userMenuAction && this.props.userMenuAction.bind(this)}
                            id={id}>
                            <CollapsibleSwitcher children={this.getChildren(user) } renderIndex={this.state.renderIndex} />
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
    userMenuAction: PropTypes.func
};
function mapStateToProps(state) {
    return {
        users: state.users.users
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(UserTable);