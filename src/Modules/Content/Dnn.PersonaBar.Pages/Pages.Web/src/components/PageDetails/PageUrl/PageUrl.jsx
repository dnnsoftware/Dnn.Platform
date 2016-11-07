import React, {Component, PropTypes} from "react";
import PageExisting from "./PageExisting/PageExisting";
import PageFile from "./PageFile/PageFile";
import PageExternalUrl from "./PageExternalUrl/PageExternalUrl";
import styles from "./style.less";

class PageUrl extends Component {

    getDetail(pageType) {        
        switch (pageType) {
            case "tab":
                return PageExisting;
            case "url":
                return PageExternalUrl;
            case "file":
                return PageFile;
            default: 
                throw "invalid page type";                                                                        
        }        
    }

    render() {
        const DetailComponent = this.getDetail(this.props.page.pageType);
        return (
            <div className={styles.pageUrl}>
                <DetailComponent {...this.props} />
            </div>
        );
    }
}

PageUrl.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageUrl;