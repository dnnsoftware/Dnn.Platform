import React, { Component } from "react";
import PropTypes from "prop-types";
import Scrollbars from "react-custom-scrollbars";

export default class PagePickerScrollbar extends Component {

    constructor(props, ...rest) {
        super(props, ...rest);
        this.renderView = this.renderView.bind(this);
        this.renderThumbDefault = this.renderThumbDefault.bind(this);
    }

    renderThumbDefault({style, ...props }) {
        const thumbStyle = {
            borderRadius: "inherit",
            height:"100%",
            backgroundColor: "rgba(204,204,204,.9)"
        };
        return <div style={{style,...thumbStyle}} {...props} />;
    }

    renderView({ style, ...props }) {
        const viewStyle = {
            padding: 15
        };
        return (
            <div
                className="box"
                style={{ ...style, ...viewStyle }}
                {...props}/>
        );
    }

    render() {
        return (
            <Scrollbars
                renderView={this.renderView}
                renderThumbVertical={this.renderThumbDefault}
                renderThumbHorizontal={this.renderThumbDefault}
                {...this.props}/>
        );
    }
}

PagePickerScrollbar.propTypes = {
    style: PropTypes.object
};