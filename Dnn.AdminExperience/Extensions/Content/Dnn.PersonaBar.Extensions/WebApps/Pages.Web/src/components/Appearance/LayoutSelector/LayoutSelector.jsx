import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class LayoutSelector extends Component {

    onClickLayout(layoutName) {
        const layout = this.props.layouts.find(l => l.name === layoutName);
        this.props.onSelectLayout(layout);
    }

    isSelected(layout) {
        const { selectedLayout } = this.props;
        if (!selectedLayout) {
            return false;
        }
        return selectedLayout.name === layout.name;
    }

    getSelectedIndex() {
        const { selectedLayout, layouts } = this.props;
        if (!selectedLayout) {
            return -1;
        }
        return layouts.findIndex(c => c.name === selectedLayout.name);
    }

    getLayoutCards() {
        const { layouts, noThemeSelected } = this.props;
        let {defaultPortalLayout} = this.props;
        defaultPortalLayout = defaultPortalLayout === null ? "" : defaultPortalLayout;

        if (noThemeSelected) {
            return <div className="no-appearance-items">{localization.get("NoThemeSelectedForLayouts") }</div>;
        }
        if (layouts.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoLayouts") }</div>;
        }
        return layouts.map(layout => {
            return <Card
                key={layout.name}
                cardId={layout.name}
                onClick={this.onClickLayout.bind(this) }
                hoverText={localization.get("SetPageLayout") }
                label={layout.name}
                isSiteDefault={defaultPortalLayout.toString().toLowerCase() === layout.path.toString().toLowerCase() }
                image={layout.thumbnail}
                selected={this.isSelected(layout) }
                size="small" />;
        });
    }

    render() {
        const selectedIndex = this.getSelectedIndex();
        return (
            <div>
                <Label
                    label={localization.get("Layout") }
                    tooltipMessage={localization.get("PageLayoutTooltip") } />
                <Gallery size="small" scrollToIndex={selectedIndex}>
                    {this.getLayoutCards() }
                </Gallery>
            </div>
        );
    }
}

LayoutSelector.propTypes = {
    noThemeSelected: PropTypes.bool.isRequired,
    selectedLayout: PropTypes.object,
    layouts: PropTypes.array.isRequired,
    onSelectLayout: PropTypes.func.isRequired,
    defaultPortalLayout: PropTypes.string
};

export default LayoutSelector;