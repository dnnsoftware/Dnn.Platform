import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import HeaderRow from "./HeaderRow";
import DetailRow from "./DetailRow";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import CollapsibleSwitcher from "../../common/CollapsibleSwitcher";
import Button from "dnn-button";
import CreateUserBox from "../CreateUserBox";
import UserSettings from "./UserSettings";
import "./style.less";

const radioButtonOptions = [
    {
        label: "Button 1",
        value: 0
    },
    {
        label: "Button 2",
        value: 1
    }
];

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
    render() {
        const {props} = this;
        let i = 0;
        let opened = (this.state.openId === "add");
        return (
            <GridCell>
                <HeaderRow  isEvoq={props.isEvoq}/>
                {opened && <DetailRow
                    Collapse={this.collapse.bind(this) }
                    OpenCollapse={this.toggle.bind(this) }
                    currentIndex={this.state.renderIndex}
                    openId={this.state.openId }
                    key={"user-add"}
                    id={"add"}
                    isEvoq={props.isEvoq}>
                    <CollapsibleSwitcher children={[<CreateUserBox onCancel={this.collapse.bind(this) }/>]}/>
                </DetailRow>
                }
                {
                    props.users && props.users.map((user, index) => {
                        let id = "row-" + i++;
                        let children = [
                            <div style={{ width: "100%", height: "300px", paddingTop: "100px", textAlign: "center" }} Collapse={this.collapse.bind(this) }>Pane 0. OpenId: {this.state.openId}
                                <Button id="cancelbtn"  type="secondary" onClick={this.collapse.bind(this) }>{Localization.get("btn_Cancel") }</Button>
                            </div>,
                            <div style={{ width: "100%", height: "300px", paddingTop: "100px", textAlign: "center" }} Collapse={this.collapse.bind(this) }>Pane 1. OpenId: {this.state.openId}<Button id="cancelbtn"  type="secondary" onClick={this.collapse.bind(this) }>{Localization.get("btn_Cancel") }</Button></div>,
                            <div style={{ width: "100%", height: "300px", paddingTop: "100px", textAlign: "center" }} Collapse={this.collapse.bind(this) }>Pane 2. OpenId: {this.state.openId}<Button id="cancelbtn"  type="secondary" onClick={this.collapse.bind(this) }>{Localization.get("btn_Cancel") }</Button></div>,
                            <UserSettings userId={user.userId} collapse={this.collapse.bind(this) } />,
                            <div style={{ width: "100%", height: "300px", paddingTop: "100px", textAlign: "center" }} Collapse={this.collapse.bind(this) }>Pane 4. OpenId: {this.state.openId}<Button id="cancelbtn"  type="secondary" onClick={this.collapse.bind(this) }>{Localization.get("btn_Cancel") }</Button></div>
                        ];
                        return <DetailRow
                            user={user}
                            Collapse={this.collapse.bind(this) }
                            OpenCollapse={this.toggle.bind(this) }
                            currentIndex={this.state.renderIndex}
                            openId={this.state.openId }
                            key={"user-" + index}
                            id={id}
                            isEvoq={props.isEvoq}>
                            <CollapsibleSwitcher children={children} renderIndex={this.state.renderIndex} />
                        </DetailRow>;
                    })
                }
            </GridCell>
        );
    }
}

UserTable.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    totalUsers: PropTypes.number,
    isEvoq: PropTypes.bool
};
UserTable.defaultProps = {
    isEvoq: false
};
function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        users: state.users.users,
        totalUsers: state.users.totalUsers
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(UserTable);