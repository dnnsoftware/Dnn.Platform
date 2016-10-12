import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import HeaderRow from "./HeaderRow";
import DetailRow from "./DetailRow";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
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
    }


    render() {
        const {props, state} = this;

        return (
            <GridCell>
                <HeaderRow />
                {props.users && props.users.map((user) => {

                    return <DetailRow user={user}/>;
                }) }
            </GridCell>
        );
    }
}

UserTable.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        users: state.users.users
    };
}

export default connect(mapStateToProps)(UserTable);