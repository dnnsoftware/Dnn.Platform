import React, {Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
import ColumnSizes from "../ExtensionColumnSizes";
import date from "../../../../utils/date";
import Collapse from "react-collapse";
import UserMenu from "../UserMenu";
import { SettingsIcon, UserIcon, MoreMenuIcon, ActivityIcon, ShieldIcon, EditIcon } from "dnn-svg-icons";

class DetailsRow extends Component {
    constructor() {
        super();
        this.handleClick = this.handleClick.bind(this);
        this.state = {
            opened: false,
            showMenu: false
        };
    }
    componentDidMount() {
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    componentWillMount() {
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
    }
    handleClick(event) {
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if (!ReactDOM.findDOMNode(this).contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)
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
        console.log("toggleUserMenu");
        const show = !this.state.showMenu;
        this.setState({ showMenu: show });
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
        return (
            /* eslint-disable react/no-danger */
            <GridCell className={"collapsible-component1"} id={uniqueId}>
                <GridCell  className={"collapsible-header1 " + !opened}>
                    <GridCell className={styles.extensionDetailRow} columnSize={100}>
                        {props.isEvoq &&
                            <GridCell columnSize={ColumnSizes[0]} className="user-avatar">
                                {user.avatar !== "-" && <img src={user.avatar}/>}
                                {user.avatar === "-" && user.avatar}
                            </GridCell>
                        }
                        <GridCell columnSize={ColumnSizes[1]} className={"user-names" + (user.isDeleted ? " deleted" : "") } style={!this.props.isEvoq && { marginLeft: "20px" }}>
                            <h6>{user.displayName}</h6>
                            {user.displayName !== "-" && <p>{user.userName}</p> }
                        </GridCell>
                        <GridCell columnSize={ColumnSizes[2]} className={user.isDeleted ? "deleted" : ""}>
                            <p>{user.email}</p>
                        </GridCell>
                        <GridCell columnSize={ColumnSizes[3]} className={user.isDeleted ? "deleted" : ""}>
                            {user.createdOnDate !== "-" && <p>{date.format(user.createdOnDate) }</p>}
                            {user.createdOnDate === "-" && user.createdOnDate}
                        </GridCell>
                        <GridCell columnSize={ColumnSizes[4]} className={user.isDeleted ? "deleted" : ""}>
                            {user.authorized !== "-" && <p>{user.authorized ? "Authorized" : "Un-authorized"}</p>}
                            {user.authorized === "-" && user.authorized}
                        </GridCell>
                        {props.id !== "add" && <GridCell columnSize={ColumnSizes[5]}>
                            <div style={{ position: "relative" }}>
                                <div className={"extension-action " + !this.state.showMenu} dangerouslySetInnerHTML={{ __html: MoreMenuIcon }}
                                    onClick={this.toggleUserMenu.bind(this) }
                                    >
                                </div>
                                { this.state.showMenu && <UserMenu onClose={this.toggleUserMenu.bind(this) } userId={user.userId}/> }
                            </div>
                            <div className={"extension-action " + !(opened && this.props.currentIndex === 3) } dangerouslySetInnerHTML={{ __html: SettingsIcon }} onClick={this.toggle.bind(this, 3) }></div>
                            <div className={"extension-action " + !(opened && this.props.currentIndex === 2) } dangerouslySetInnerHTML={{ __html: UserIcon }} onClick={this.toggle.bind(this, 2) }></div>
                            <div className={"extension-action " + !(opened && this.props.currentIndex === 1) } dangerouslySetInnerHTML={{ __html: ShieldIcon }} onClick={this.toggle.bind(this, 1) }></div>
                            {props.isEvoq &&
                                <div className={"extension-action " + !(opened && this.props.currentIndex === 0) } dangerouslySetInnerHTML={{ __html: ActivityIcon }} onClick={this.toggle.bind(this, 0) }></div>
                            }
                        </GridCell>
                        }
                        {
                            // props.id === "add"
                            // &&
                            // <GridCell columnSize={ColumnSizes[5]}>
                            //     <div className={"extension-action false"}
                            //         dangerouslySetInnerHTML={{ __html: EditIcon }}
                            //         onClick={this.toggle.bind(this, 0) }>
                            //     </div>
                            // </GridCell>
                        }
                        <Collapse accordion={true} isOpened={opened} keepCollapsedContent={true} className="user-detail-row">
                            {opened && props.children }
                        </Collapse>
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
    isEvoq: PropTypes.bool
};
DetailsRow.defaultProps = {
    isEvoq: false
};

export default DetailsRow;