import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import Localization from "../../localization";
import RadioButtons from "dnn-radio-buttons";
import utils from "../../utils";

class PageTypeSelector extends Component {

    getComponents() {
        const {props} = this;
        if (props.components && props.components.length > 0) {
            return this.props.components.map(function (component) {
                const Component = component.component;
                return <Component page={props.page} />;
            });
        }
        return false;
    }

    render() {
        const {page, onChangePageType} = this.props;
        const createdDate = Localization.get("CreatedValue")
                                .replace("[CREATEDDATE]", utils.formatDateNoTime(page.createdOnDate))
                                .replace("[CREATEDUSER]", page.created || "System");
        const hierarchy = page.hierarchy === "" ? Localization.get("TopPage") : page.hierarchy;
        const components = this.getComponents(); 

        return (
            <div className={styles.pageTypeSelector}>
                <div>
                    {components}
                </div>
                <div>
                    <div className="page-info-row page-name">
                        {page.name}
                    </div>
                    <div className="page-info-row">
                        <div className="page-info-item">
                            <div className="page-info-item-label">
                                {Localization.get("Created") + ": "}
                            </div>
                            <div className="page-info-item-value">
                                {createdDate}
                            </div>
                        </div>
                        <div className="page-info-item">
                            <div className="page-info-item-label">
                                {Localization.get("PageParent") + ": "}
                            </div>
                            <div className="page-info-item-value">
                                {hierarchy}
                            </div>
                        </div>
                        <div className="page-info-item">
                            <div className="page-info-item-label">
                                {Localization.get("Status") + ": "}
                            </div>
                            <div className="page-info-item-value">
                                {Localization.get("Status_" + page.status)}
                            </div>
                        </div>
                    </div>
                    <div className="page-info-row page-type">
                        <div className="page-info-item">
                            <div className="page-info-item-label">
                                {Localization.get("PageType") + ": "}
                            </div>
                            <div className="page-info-item-value">
                                <RadioButtons 
                                    options={[{value: "normal", label: Localization.get("Standard")}, 
                                            {value: "tab", label: Localization.get("Existing")},
                                            {value: "url", label: Localization.get("Url")},
                                            {value: "file", label: Localization.get("File")}]} 
                                    onChange={onChangePageType}
                                    value={page.pageType}/>
                            </div>
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

export default PageTypeSelector;