import React, {PropTypes} from "react";
import KoComponent from "./KoComponent";

class KoTest extends KoComponent {
    render() {
        return (
            <ul data-bind="foreach: props.pages">
                <li data-bind="text: $data"></li>
            </ul>
        );
    }
} 

KoTest.propTypes = {
    pages: PropTypes.array.isRequired
};

export default KoTest; 
