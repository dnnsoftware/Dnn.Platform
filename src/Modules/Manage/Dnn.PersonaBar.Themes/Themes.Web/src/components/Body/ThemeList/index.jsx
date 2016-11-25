import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import GridCell from "dnn-grid-cell";
import { Scrollbars } from "react-custom-scrollbars";

import Theme from "./Theme";

import "./style.less";

class ThemeList extends Component {
    constructor() {
        super();
        this.state = {};
    }
    
    render() {
        const {props} = this;

        return (
            <Scrollbars
                    className="theme-list"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={480}>
                <GridCell>
                    {props.dataSource.map((theme) => {
                        return <GridCell columnSize="25"><Theme theme={theme} /></GridCell>;
                    }) }
                </GridCell>
            </Scrollbars>
        );
    }
}

ThemeList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    dataSource: PropTypes.array
};

function mapStateToProps(state) {
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeList);