import React, {Component, PropTypes} from "react";

const style = {    
    cursor: "pointer",
    width: 13,
    height: 13,
    disabled: {
        cursor: "default",
        opacity: 0.5
    }
};

const wrapperStyle = {    
    float: "left",
    padding: "10px 11px 10px 11px",   
    borderRight: "solid 1px #c8c8c8"
};

class Button extends Component {
    getStyle() {
        return Object.assign({}, style, this.props.disabled && style.disabled);
    }
    
    /* eslint-disable react/no-danger */
    render() {
        return (
            <div style={wrapperStyle}>
                <div style={this.getStyle()} onClick={this.props.onClick} dangerouslySetInnerHTML={{ __html: this.props.icon }}></div>     
            </div>       
        );
    }
}

Button.propTypes = {
    icon: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    onClick: PropTypes.func.isRequired
};

export default Button;