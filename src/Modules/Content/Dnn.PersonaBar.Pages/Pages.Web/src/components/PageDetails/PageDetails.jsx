import React, {Component, PropTypes} from "react";
import PageStandard from "./PageStandard/PageStandard";
import PageUrl from "./PageUrl/PageUrl";
import PageDetailsFooter from "./PageDetailsFooter/PageDetailsFooter";

class PageDetail extends Component {

    getDetail(pageType) {        
        switch (pageType) {
            case "normal": 
                return PageStandard;
            case "tab":
            case "url":
            case "file":
                return PageUrl;
            default: 
                throw "invalid page type";
        }        
    }

    render() {
        const DetailComponent = this.getDetail(this.props.page.pageType);

        return (
            <div>
                <DetailComponent {...this.props} />
                <PageDetailsFooter {...this.props} />
            </div>
        );
    }
}

PageDetail.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired
};

export default PageDetail;