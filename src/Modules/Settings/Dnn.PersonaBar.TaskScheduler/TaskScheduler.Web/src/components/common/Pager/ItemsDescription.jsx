import React, {Component, PropTypes} from "react";
import resx from "../../../resources";

const style = {        
    float: "left",
    padding: "10px 0 0 0"
};

class ItemsDescription extends Component {
    getDescription() {
        if (this.props.total === 0) {
            return resx.get("pagerNoEntries");
        }
        
        if (this.props.total === 1) {
            return resx.get("pagerShowOneEntry");
        }

        if (this.props.total <= this.props.pageSize) {
            const noPagerFormat = resx.get("noPagerFormat");
            return noPagerFormat.replace("[TOTAL]", this.props.total);            
        }
        
        const pagerFormat = resx.get("pagerFormat");
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