import React, {Component, PropTypes} from "react";
import Dropdown from "dnn-dropdown";

class componentName extends Component {
    getItemsPerPage() {
        return (
            <Dropdown
                options={this.props.pageSizeOptions}
                value={this.props.pageSize.toString() }
                onSelect={this.props.onPageSizeChange.bind(this) }
                style={this.props.pageSizeStyle}/>
        );
    }

    render() {
        const {props} = this;
        return (
            <div>
                {this.getItemsPerPage() }
            </div>
        );
    }
}

componentName.propTypes = {
    pageSizeOptions: PropTypes.array.isRequired,
    pageSize: PropTypes.number.isRequired,
    onPageSizeChange: PropTypes.func.isRequired,
    pageSizeStyle: PropTypes.object
};

export default componentName;