import React, {Component, PropTypes} from "react";

const style = {        
    float: "left",
    width: "12px",
    maxWidth: "12px",
    display: "inline-block",
    padding: "9px 12px 7px 12px",
    borderRight: "solid 1px #ddd",
    textAlign: "center"
};

class componentName extends Component {
    getDescription() { 
        const page = "[PAGE]";         
        return page.replace("[PAGE]", this.props.currentPage);       
    }
    
    render() {
        return (
            <div style={style}>{this.getDescription()}</div>
        );
    }
}

componentName.propTypes = {
    currentPage: PropTypes.number.isRequired
};

export default componentName;