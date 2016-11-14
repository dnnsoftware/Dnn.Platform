import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import PageNameInput from "./PageNameInput";
import DisplayInMenu from "./DisplayInMenu";
import EnableScheduling from "./EnableScheduling";
import application from "../../../globals/application";

class PageDetailsFooter extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onChangeValue(key, value) {
        const {onChangeField} = this.props;
        onChangeField(key, value);
    }

    getLeftColumnComponents(normalPage, pageType) {
        const {page, errors} = this.props;
        const defaultLeftColumnComponents = !normalPage ?
            [<PageNameInput pageName={page.name}
                errors={errors}
                onChangePageName={this.onChangeField.bind(this, "name")} />] :
            [<DisplayInMenu includeInMenu={page.includeInMenu}
                onChangeIncludeInMenu={this.onChangeValue.bind(this, "includeInMenu")} />];

        const additionalLeftComponents = application.getPageDetailFooterComponents().filter(
            function (component) {
                return component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultLeftColumnComponents, additionalLeftComponents, "order", "component");
        return defaultLeftColumnComponents;
    }

    insertElementsInArray(array, elements, propertyNameHasIndex, propertyNameHasValue) {
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i][propertyNameHasIndex]);
            const Component = elements[i][propertyNameHasValue];
            Component.props.page = this.props.page;
            Component.props.onChange = this.onChangeValue.bind(this);

            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, [Component, <div style={{ clear: "both" }}></div>]);
            }            
        }
    }

    getInteger(value) {
        if (value === 0) {
            return 0;
        }
        if (value) {
            return parseInt(value.toString());
        }
        return value;
    }  

    getRightColumnComponents(normalPage, pageType) {
        const {page} = this.props;
        const defaultRightColumnComponents = !normalPage ? 
            [<DisplayInMenu includeInMenu={page.includeInMenu}
                onChangeIncludeInMenu={this.onChangeValue.bind(this, "includeInMenu")} />,
            <EnableScheduling schedulingEnabled={page.schedulingEnabled}
                onChangeSchedulingEnabled={this.onChangeValue.bind(this, "schedulingEnabled")}
                startDate={page.startDate}
                endDate={page.endDate}
                onChangeStartDate={this.onChangeValue.bind(this, "startDate")}
                onChangeEndDate={this.onChangeValue.bind(this, "endDate")} />] :
            [<EnableScheduling schedulingEnabled={page.schedulingEnabled}
                onChangeSchedulingEnabled={this.onChangeValue.bind(this, "schedulingEnabled")}
                startDate={page.startDate}
                endDate={page.endDate}
                onChangeStartDate={this.onChangeValue.bind(this, "startDate")}
                onChangeEndDate={this.onChangeValue.bind(this, "endDate")} />];
        
        const additionalRightComponents = application.getPageDetailFooterComponents().filter(
            function (component) {
                return !component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultRightColumnComponents, additionalRightComponents, "order", "component");
        return defaultRightColumnComponents;
    }

    render() {
        const {page} = this.props;
        const normalPage = page.pageType === "normal";

        return (
            <div className={styles.pageDetailsFooter}>                
                <GridSystem>
                    <GridCell className="left-column">                        
                        {this.getLeftColumnComponents(normalPage, page.pageType)}
                    </GridCell>
                    <GridCell className="right-column">                       
                        {this.getRightColumnComponents(normalPage, page.pageType)}                       
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }
}

PageDetailsFooter.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetailsFooter;