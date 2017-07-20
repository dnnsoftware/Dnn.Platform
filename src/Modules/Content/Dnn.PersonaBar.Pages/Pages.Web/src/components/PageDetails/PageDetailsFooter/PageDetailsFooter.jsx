import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import PageNameInput from "./PageNameInput";
import DisplayInMenu from "./DisplayInMenu";
import EnableScheduling from "./EnableScheduling";
import Template from "./Template";

class PageDetailsFooter extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        if (event.value) {
            onChangeField(key, event.value);
        } else {
            onChangeField(key, event.target.value);
        }        
    }

    onChangeValue(key, value) {
        const {onChangeField} = this.props;
        onChangeField(key, value);
    }

    getLeftColumnComponents(normalPage, pageType, includeTemplates) {
        const {page, errors} = this.props;
        let defaultLeftColumnComponents;
        if (!normalPage) {
            defaultLeftColumnComponents = [<PageNameInput pageName={page.name}
                errors={errors}
                onChangePageName={this.onChangeField.bind(this, "name")} />];
        } else {
            defaultLeftColumnComponents = [<DisplayInMenu includeInMenu={page.includeInMenu}
                onChangeIncludeInMenu={this.onChangeValue.bind(this, "includeInMenu")} />];
            if (includeTemplates && page.tabId === 0) {
                defaultLeftColumnComponents.push(
                    <Template templates={page.templates} 
                        selectedTemplateId={page.templateId}
                        onSelect={this.onChangeField.bind(this, "templateId")} />
                );
            }
        }

        const additionalLeftComponents = this.props.components.filter(
            function (component) {
                return !component.newSection && component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultLeftColumnComponents, additionalLeftComponents);
        return defaultLeftColumnComponents;
    }

    insertElementsInArray(array, elements) {
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i].order);
            const Component = elements[i].component;
            const instance = <Component page={this.props.page} onChange={this.onChangeValue.bind(this)} 
                store={elements[i].store} />;

            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, instance);
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
        
        const additionalRightComponents = this.props.components.filter(
            function (component) {
                return !component.newSection && !component.leftSide && (component.pageType === pageType || component.pageType === "all");
            });

        this.insertElementsInArray(defaultRightColumnComponents, additionalRightComponents, "order", "component");
        return defaultRightColumnComponents;
    }
    
    getNewSections(normalPage, pageType) {
        const additionalComponents = this.props.components.filter(
            function (component) {
                return component.newSection && (component.pageType === pageType || component.pageType === "all");
            });
            
        let orderedComponents = [];
        this.insertElementsInArray(orderedComponents, additionalComponents, "order", "component");
        return orderedComponents;
    }

    render() {
        const {page} = this.props;
        const normalPage = page.pageType === "normal";
        const componentsInNewSection= this.getNewSections(normalPage, page.pageType);
        const includeTemplates = componentsInNewSection.length === 0;

        return (
            <div className={styles.pageDetailsFooter}>                
                <GridSystem>
                    <GridCell className="left-column">                        
                        {this.getLeftColumnComponents(normalPage, page.pageType, includeTemplates)}
                    </GridCell>
                    <GridCell className="right-column">                       
                        {this.getRightColumnComponents(normalPage, page.pageType)}                       
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
                {componentsInNewSection}
            </div>
        );
    }
}

PageDetailsFooter.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired
};

export default PageDetailsFooter;