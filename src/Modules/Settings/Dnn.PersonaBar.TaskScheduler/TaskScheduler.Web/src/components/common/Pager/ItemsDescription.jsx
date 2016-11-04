import React, {Component, PropTypes} from "react";

const style = {        
    float: "left",
    padding: "10px 0 0 0"
};

class ItemsDescription extends Component {
    getDescription() {
        if (this.props.total === 0) {
            return "No entries";
        }
        
        if (this.props.total === 1) {
            return "Showing 1 entry";
        }

        if (this.props.total <= this.props.pageSize) {
            const noPagerFormat = "Showing [TOTAL] entries";
            return noPagerFormat.replace("[TOTAL]", this.props.total);            
        }
        
        const pagerFormat = "Showing [FROM]-[TO] of [TOTAL] entries";
        return pagerFormat.replace("[FROM]", this.props.from)
                            .replace("[TO]", this.props.to)
                            .replace("[TOTAL]", this.props.total);
    }
    
    render() {
        return (
            <div style={style}>{this.getDescription()}</div>
        );
    }
}

ItemsDescription.propTypes = {
    from : PropTypes.number.isRequired,
    to : PropTypes.number.isRequired,
    total: PropTypes.number.isRequired,
    pageSize: PropTypes.number.isRequired
};

export default ItemsDescription;