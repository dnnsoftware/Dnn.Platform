import React, {Component, PropTypes} from "react";
import styles from "./style.less";

class PageExternalUrl extends Component {

    render() {
        const {page} = this.props;
        
        return (
            <div className={styles.pageExternalUrl}>
                
            </div>
        );
    }
}

PageExternalUrl.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageExternalUrl;