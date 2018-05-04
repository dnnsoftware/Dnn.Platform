import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";

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
                <DetailComponent onChangeField={this.props.onChangeField} />
            </div>
        );
    }
}

PageUrl.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
    return ({
        page: state.pages.selectedPage
    });
};

export default connect(mapStateToProps)(PageUrl);