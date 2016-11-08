import React, {Component, PropTypes} from "react";

class Appearance extends Component {

    render() {        
        return (
            <div>Appearance</div>
        );
    }
}

Appearance.propTypes = {
    page: PropTypes.object.isRequired
};

export default Appearance;
