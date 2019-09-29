import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.less";
import Localization from "../../localization";
import { RadioButtons } from "@dnnsoftware/dnn-react-common";
import utils from "../../utils";


const MAX_PAGE_PARENT_SIZE_TOOGLE = 180;

class PageTypeSelector extends Component {

    constructor(props) {
        super(props);
        this.pageParentLarge = false;
    }

    getComponents() {
        const {props} = this;
        if (props.components && props.components.length > 0) {
            return this.props.components.map((component, index) => {
                const Component = component.component;
                return <Component key={index} page={props.page} />;
            });
        }
        return false;
    }
    _getHierarchyLabel() {
        const page = this.props.page;
        if (page.hierarchy === null || page.hierarchy === "" || page.hierarchy === Localization.get("NoneSpecified")) {
            return Localization.get("TopPage");
        } else {
            return page.hierarchy;
        }
    }

    componentDidMount() {
        const pageSelectorDOM = this.node;
        if (pageSelectorDOM.querySelector("#parentPageValue").offsetWidth > MAX_PAGE_PARENT_SIZE_TOOGLE) {
            pageSelectorDOM.querySelector("#pageParent").className = "page-info-item page-parent-info-style-large";
            pageSelectorDOM.querySelector("#pageParentLabel").className = "page-info-item-label parent-page-style-label-large";
            pageSelectorDOM.querySelector("#pageParentContent").className = "page-info-item-value parent-page-name parent-page-style-content-large";
        }
    }

    _calculateParentPageSize(parentPageRef) {
        if (parentPageRef !== null) {
            this.pageParentLarge = (parentPageRef.offsetWidth > MAX_PAGE_PARENT_SIZE_TOOGLE);
        }
    }

    _defineVisibleOrHidden(includeInMenu) {
        return includeInMenu ? Localization.get("Status_Visible") : Localization.get("Status_Hidden");
    }


    render() {
        const { page, onChangePageType } = this.props;
        const createdDate = Localization.get("CreatedValue")
                                .replace("[CREATEDDATE]", utils.formatDateNoTime(page.createdOnDate))
                                .replace("[CREATEDUSER]", page.created || "System");
        
        const hierarchy = this._getHierarchyLabel();        
        const components = this.getComponents();
                
        return (
            <div className={styles.pageTypeSelector} ref={node => this.node = node}>
                <div>
                    {components}
                </div>
                <div>
                    <div className="page-info-row page-name">
                        {page.name}
                    </div>
                    <div className="page-info-row">
                        <div className="page-info-item">
                            <span className="page-info-item-label">
                                {Localization.get("Created") + ": "}
                            </span>
                            <span className="page-info-item-value">
                                {createdDate}
                            </span>
                        </div>
                        <div id="pageParent" className={this.pageParentLarge ? "page-info-item page-parent-info-style-large" : "page-info-item"}>
                            <span id="pageParentLabel" className={this.pageParentLarge ? "page-info-item-label parent-page-style-label-large" : "page-info-item-label"}>
                                {Localization.get("PageParent") + ": "}
                            </span>
                            <span id="pageParentContent" className={this.pageParentLarge ? "page-info-item-value parent-page-name parent-page-style-content-large" : "page-info-item-value parent-page-name"}>
                                 <span id="parentPageValue" ref={(parentPageRef)=>this._calculateParentPageSize(parentPageRef)} >
                                    {hierarchy}
                                </span>
                            </span>
                        </div>
                        <div className="page-info-item">
                            <span className="page-info-item-label">
                                {Localization.get("Status") + ": "}
                            </span>
                            <span className="page-info-item-value">
                                {this._defineVisibleOrHidden(page.includeInMenu)}
                            </span>
                        </div>
                    </div>
                    <div className="page-info-row">
                        <div className="page-info-item page-type">
                            <span className="page-info-item-label">
                                {Localization.get("PageType") + ": "}
                            </span>
                            <span className="page-info-item-value">
                                <RadioButtons
                                    options={[{value: "normal", label: Localization.get("Standard")},
                                            {value: "tab", label: Localization.get("Existing")},
                                            {value: "url", label: Localization.get("Url")},
                                            {value: "file", label: Localization.get("File")}]}
                                    onChange={onChangePageType}
                                    value={page.pageType}/>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

PageTypeSelector.propTypes = {
    page: PropTypes.object.isRequired,
    onChangePageType: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired
};

const mapStateToProps = (state) => {
    return {
        page : state.pages.selectedPage
    };
};

export default connect(mapStateToProps)(PageTypeSelector);