import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    pagination as PaginationActions
} from "actions";
import Localization from "localization";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import Dropdown from "dnn-dropdown";
import SearchBox from "dnn-search-box";
import UserTable from "./UserTable";
import CreateUserBox from "./CreateUserBox";
import Collapse from "react-collapse";
import "./style.less";

class Body extends Component {
    constructor() {
        super();
        this.state = {
            selectedRadioButton: 0,
            value: "Test Me!",
            textAreaValue: "Multi line!",
            singleLineValue: "Single line!",
            selectValue: 1
        };
    }
    componentWillMount() {
        console.log(Localization.get("nav_Users"));
        // const {props} = this;
        //props.dispatch(); //Dispatch action to get data here
    }

    onRadioButtonChange(value) {
        alert(value);
    }

    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onSelectChange(option) {
        this.setState({
            selectValue: option.value
        });
    }

    getWorkSpaceTray() {
        return <GridCell className="users-workspace-tray">
            <GridCell columnSize={33}>
                <Dropdown options={[
                    {
                        label: "Registered Users",
                        value: "Registered Users"
                    }
                ]}
                    withBorder={false}
                    value="Registered Users"
                    prependWith="Show: "/>
            </GridCell>
            <GridCell columnSize={33}>
                <Dropdown options={[
                    {
                        label: "Registered Users",
                        value: "Registered Users"
                    }
                ]}
                    withBorder={false}
                    value="Registered Users"
                    prependWith="Showing: "/>
            </GridCell>
            <GridCell columnSize={33}>
                <SearchBox style={{ height: 29 }}/>
            </GridCell>
        </GridCell>;
    }

    toggleCreateBox() {
        this.setState({
            createBoxVisible: !this.state.createBoxVisible
        });
    }

    render() {
        const {props, state} = this;
        const panelBodyMargin = state.createBoxVisible ? "without-margin" : "";
        return (
            <GridCell>
                <SocialPanelHeader title={Localization.get("nav_Users") }>
                    <Button type="primary" size="large" onClick={this.toggleCreateBox.bind(this) }>{Localization.get("btn_CreateUser") }</Button>
                </SocialPanelHeader>
                <Collapse className="create-user-box-collapse" isOpened={state.createBoxVisible} keepCollapsedContent={true}><CreateUserBox /></Collapse>
                <SocialPanelBody workSpaceTrayVisible={true} workSpaceTrayOutside={true} workSpaceTray={this.getWorkSpaceTray() } className={panelBodyMargin}>
                    <UserTable/>
                </SocialPanelBody >
            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(Body);