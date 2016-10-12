import React, {Component, PropTypes} from "react";
import MultiPersonaBarPage from "./multiPersonaBarPage";
import PersonaBarPage from "./personaBarPage";

class PersonaBarPagePagesContainer extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;
        const pages = props.pages.map((page, index) => {
            if (page.length && page.length > 0) {
                return <MultiPersonaBarPage left={index * 100} top={props.repaintChildren ? 0 : (props.selectedPageVisibleIndex * -100) }>
                    {page.map((_page, _index) => {
                        return (
                            <PersonaBarPage childPage={true} top={props.repaintChildren ? 0 : (_index * 100)}>
                                {props.repaintChildren ? (_index === props.selectedPageVisibleIndex && _page) : _page}
                            </PersonaBarPage>
                        );
                    }) }
                </MultiPersonaBarPage>;
            } else {
                return (
                    <PersonaBarPage left={index * 100}>
                        {page}
                    </PersonaBarPage>
                );
            }
        });
        return (
            <div className="personaBar-pagesContainer" style={{ left: props.selectedPage * -100 + "%" }}>
                {pages}
            </div>
        );
    }
}

PersonaBarPagePagesContainer.PropTypes = {
    pages: PropTypes.array,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};

export default PersonaBarPagePagesContainer;