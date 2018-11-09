import React, {PropTypes, Component} from "react";

const getUrl = (code: string) => {
    try {
        return require(`./img/flags/${code}.png`);
    } catch (error) {
        return require("./img/flags/none.png");
    }
};

const getStyle = (code: string, isGeneric: boolean) => ({
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
        const isGeneric: boolean = !culture.includes("-");
        return <div
            onClick={onClick}
            title={title}
            style={getStyle(culture, isGeneric)}>{isGeneric ? culture : ""}</div>;
    }
}

Flag.PropTypes = {
    culture: PropTypes.string,
    onClick: PropTypes.function,
    title: PropTypes.string,
};

export default Flag;
