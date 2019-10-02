import React, { Component } from "react";
import PropTypes from "prop-types";

const getUrl = (code) => {
    try {
        return require(`./img/flags/${code}.png`);
    } catch (error) {
        return require("./img/flags/none.png");
    }
};

const getStyle = (code, isGeneric) => ({
    backgroundColor: isGeneric ? "#78BEDB" : "transparent",
    backgroundRepeat: "no-repeat",
    backgroundImage: isGeneric ? "none" : `url(${getUrl(code)})`,
    backgroundPositionY: "50%",
    backgroundPositionX: "50%",
    color: "#FFF",
    textTransform: "uppercase",
    display: "inline-block",
    marginRight: "5px",
    fontWeight: "bold",
    width: "27px",
    height: "18px",
    lineHeight: "18px",
    verticalAlign: "middle",
    textAlign: "center"
});

class Flag extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const isGeneric = !this.props.culture.includes("-");
        return <div
            onClick={this.props.onClick}
            title={this.props.title}
            style={getStyle(this.props.culture, isGeneric)}>{isGeneric ? this.props.culture : ""}</div>;
    }
}

Flag.propTypes = {
    culture: PropTypes.string,
    onClick: PropTypes.func,
    title: PropTypes.string,
};

export default Flag;
